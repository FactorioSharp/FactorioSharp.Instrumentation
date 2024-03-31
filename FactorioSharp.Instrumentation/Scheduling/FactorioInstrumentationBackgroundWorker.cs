using System.Diagnostics.Metrics;
using FactorioSharp.Instrumentation.Integration;
using FactorioSharp.Instrumentation.Integration.Jobs;
using FactorioSharp.Instrumentation.Meters;
using FactorioSharp.Instrumentation.Model;
using FactorioSharp.Rcon;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FactorioSharp.Instrumentation.Scheduling;

/// <summary>
///     Collect data from the factorio server periodically and emit it on the available meters
/// </summary>
class FactorioInstrumentationBackgroundWorker : BackgroundService
{
    public static readonly string MeterName = typeof(FactorioInstruments).Assembly.GetName().Name!;
    public static readonly string MeterVersion = typeof(FactorioInstruments).Assembly.GetName().Version!.ToString();

    readonly FactorioRconClientProvider _clientProvider;
    bool _isConnected;
    readonly FactorioMeterOptionsInternal _options;
    readonly ILogger<FactorioInstrumentationBackgroundWorker> _logger;
    readonly FactorioServerData _cache;
    readonly JobCollection _jobs;
    Meter? _meter;

    public FactorioInstrumentationBackgroundWorker(string host, int port, string password, IOptions<FactorioMeterOptions> options, ILoggerFactory loggerFactory)
    {
        _clientProvider = new FactorioRconClientProvider(host, port, password, loggerFactory.CreateLogger<FactorioRconClientProvider>());
        _options = new FactorioMeterOptionsInternal(options.Value);
        _logger = loggerFactory.CreateLogger<FactorioInstrumentationBackgroundWorker>();

        _cache = new FactorioServerData();
        _meter = CreateMeter();

        _jobs = new JobCollection
        {
            new UpdateForcesToMeasureJob(loggerFactory.CreateLogger<UpdateForcesToMeasureJob>()),
            new UpdateItemsToMeasureJob(loggerFactory.CreateLogger<UpdateItemsToMeasureJob>()),
            new UpdateFluidsToMeasureJob(loggerFactory.CreateLogger<UpdateFluidsToMeasureJob>()),
            new UpdateItemsJob(),
            new UpdateFluidsJob()
        };
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting background worker...");
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping background worker...");
        _clientProvider.Dispose();
        _meter?.Dispose();
        return base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        TimeSpan minDelayBetweenObservations = TimeSpan.FromSeconds(1);
        TimeSpan minDelayBetweenConnectionAttempts = TimeSpan.FromSeconds(10);

        await _jobs.ExecuteOnStartAsync(_cache, _options, stoppingToken);

        _isConnected = false;

        while (!stoppingToken.IsCancellationRequested)
        {
            DateTime startTime = DateTime.Now;

            FactorioRconClientProvider.GetConnectedClientResult result = await _clientProvider.TryGetConnectedClient();
            if (result.Succeeded)
            {
                await HandleTick(result.Client!, stoppingToken);

                TimeSpan elapsed = DateTime.Now - startTime;
                TimeSpan toWait = minDelayBetweenObservations - elapsed;

                if (toWait > TimeSpan.Zero)
                {
                    await Task.Delay(toWait, stoppingToken);
                }
            }
            else
            {
                await HandleConnectionFailed(result, stoppingToken);
                await Task.Delay(minDelayBetweenConnectionAttempts, stoppingToken);
            }
        }

        await _jobs.ExecuteOnStopAsync(_cache, _options, stoppingToken);
    }

    async Task HandleTick(FactorioRconClient client, CancellationToken stoppingToken)
    {
        if (!_isConnected)
        {
            _isConnected = true;
            await _jobs.ExecuteOnConnectAsync(_cache, client, _options, stoppingToken);

            _meter ??= CreateMeter();
            FactorioInstruments.Setup(_meter, _cache, _options);
        }

        await _jobs.ExecuteOnTickAsync(_cache, client, _options, stoppingToken);
    }

    async Task HandleConnectionFailed(FactorioRconClientProvider.GetConnectedClientResult result, CancellationToken stoppingToken)
    {
        if (_isConnected)
        {
            _isConnected = false;
            await _jobs.ExecuteOnDisconnectAsync(_cache, _options, stoppingToken);

            _meter?.Dispose();
            _meter = CreateMeter();
        }

        _logger.LogError(result.Exception, "Could not connect to server at {host}:{port}. Reason: {reason}.", result.Host, result.Port, result.FailureReason);
    }

    Meter CreateMeter()
    {
        Meter meter = new(MeterName, MeterVersion);
        meter.CreateObservableGauge("factorio.server.status", () => _isConnected ? 1 : 0, "1", "The current status of the factorio server");

        return meter;
    }
}
