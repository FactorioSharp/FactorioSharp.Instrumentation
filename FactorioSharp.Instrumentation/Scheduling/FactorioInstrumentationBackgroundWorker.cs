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
    readonly FactorioRconClientProvider _clientProvider;
    bool _isConnected;
    readonly FactorioMeterOptionsInternal _options;
    readonly ILogger<FactorioInstrumentationBackgroundWorker> _logger;
    readonly FactorioData _data;
    readonly JobCollection _jobs;
    Meter? _meter;

    public FactorioInstrumentationBackgroundWorker(IOptions<FactorioInstrumentationOptions> options, ILoggerFactory loggerFactory)
    {


        _clientProvider = new FactorioRconClientProvider(
            options.Value.Server.Uri ?? throw new ArgumentNullException(nameof(options.Value.Server.Uri)),
            options.Value.Server.RconPassword ?? throw new ArgumentNullException(nameof(options.Value.Server.Uri)),
            loggerFactory.CreateLogger<FactorioRconClientProvider>()
        );
        _options = new FactorioMeterOptionsInternal(options.Value.Meter);
        _logger = loggerFactory.CreateLogger<FactorioInstrumentationBackgroundWorker>();

        _data = new FactorioData(options.Value.Server.Uri, options.Value.Server.Name);

        _meter = CreateMeter();
        FactorioServerInstruments.Setup(_meter, _data.Server, _options);

        _jobs = new JobCollection(loggerFactory.CreateLogger<JobCollection>())
        {
            new UpdateFactorioServerStatus(loggerFactory.CreateLogger<UpdateFactorioServerStatus>()),
            new UpdateFactorioServerMods(loggerFactory.CreateLogger<UpdateFactorioServerMods>()),
            new UpdateFactorioServerPlayers(loggerFactory.CreateLogger<UpdateFactorioServerPlayers>()),
            new UpdateForcesToMeasureJob(loggerFactory.CreateLogger<UpdateForcesToMeasureJob>()),
            new UpdateItemsToMeasureJob(loggerFactory.CreateLogger<UpdateItemsToMeasureJob>()),
            new UpdateFluidsToMeasureJob(loggerFactory.CreateLogger<UpdateFluidsToMeasureJob>()),
            new UpdateItemsJob(loggerFactory.CreateLogger<UpdateItemsJob>()),
            new UpdateFluidsJob(loggerFactory.CreateLogger<UpdateFluidsJob>())
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

        await _jobs.ExecuteOnStartAsync(_data, _options, stoppingToken);

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

        await _jobs.ExecuteOnStopAsync(_data, _options, stoppingToken);
    }

    async Task HandleTick(FactorioRconClient client, CancellationToken stoppingToken)
    {
        if (!_isConnected)
        {
            _isConnected = true;

            await _jobs.ExecuteOnConnectAsync(_data, client, _options, stoppingToken);

            if (_meter == null)
            {
                _meter = CreateMeter();
                FactorioServerInstruments.Setup(_meter, _data.Server, _options);
            }

            FactorioGameInstruments.Setup(_meter, _data, _options);
        }

        await _jobs.ExecuteOnTickAsync(_data, client, _options, stoppingToken);
    }

    async Task HandleConnectionFailed(FactorioRconClientProvider.GetConnectedClientResult result, CancellationToken stoppingToken)
    {
        if (_isConnected)
        {
            _isConnected = false;

            await _jobs.ExecuteOnDisconnectAsync(_data, _options, stoppingToken);

            _meter?.Dispose();
            _meter = CreateMeter();
            FactorioServerInstruments.Setup(_meter, _data.Server, _options);
        }

        _logger.LogError(result.Exception, "Could not connect to server at {host}:{port}. Reason: {reason}.", result.Uri.Host, result.Uri.Port, result.FailureReason);
    }

    Meter CreateMeter() => new(FactorioInstrumentationConstants.MeterName, FactorioInstrumentationConstants.MeterVersion);
}
