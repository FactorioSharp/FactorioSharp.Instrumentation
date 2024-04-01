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
    public static readonly string MeterName = typeof(FactorioGameInstruments).Assembly.GetName().Name!;
    public static readonly string MeterVersion = typeof(FactorioGameInstruments).Assembly.GetName().Version!.ToString();

    readonly FactorioRconClientProvider _clientProvider;
    bool _isConnected;
    readonly FactorioMeterOptionsInternal _options;
    readonly ILogger<FactorioInstrumentationBackgroundWorker> _logger;
    readonly FactorioServerData _serverData;
    readonly FactorioGameData _gameData;
    readonly JobCollection _jobs;
    Meter? _meter;

    public FactorioInstrumentationBackgroundWorker(string host, int port, string password, IOptions<FactorioMeterOptions> options, ILoggerFactory loggerFactory)
    {
        _clientProvider = new FactorioRconClientProvider(host, port, password, loggerFactory.CreateLogger<FactorioRconClientProvider>());
        _options = new FactorioMeterOptionsInternal(options.Value);
        _logger = loggerFactory.CreateLogger<FactorioInstrumentationBackgroundWorker>();

        _serverData = new FactorioServerData();
        _gameData = new FactorioGameData();

        _meter = CreateMeter();
        FactorioServerInstruments.Setup(_meter, _serverData, _options);

        _jobs = new JobCollection
        {
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

        await _jobs.ExecuteOnStartAsync(_gameData, _options, stoppingToken);

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

        await _jobs.ExecuteOnStopAsync(_gameData, _options, stoppingToken);
    }

    async Task HandleTick(FactorioRconClient client, CancellationToken stoppingToken)
    {
        if (!_isConnected)
        {
            _isConnected = true;
            _serverData.IsConnected = true;

            await _jobs.ExecuteOnConnectAsync(_gameData, client, _options, stoppingToken);

            if (_meter == null)
            {
                _meter = CreateMeter();
                FactorioServerInstruments.Setup(_meter, _serverData, _options);
            }

            FactorioGameInstruments.Setup(_meter, _gameData, _options);
        }

        await _jobs.ExecuteOnTickAsync(_gameData, client, _options, stoppingToken);
    }

    async Task HandleConnectionFailed(FactorioRconClientProvider.GetConnectedClientResult result, CancellationToken stoppingToken)
    {
        if (_isConnected)
        {
            _isConnected = false;
            _serverData.IsConnected = false;

            await _jobs.ExecuteOnDisconnectAsync(_gameData, _options, stoppingToken);

            _meter?.Dispose();
            _meter = CreateMeter();
            FactorioServerInstruments.Setup(_meter, _serverData, _options);
        }

        _logger.LogError(result.Exception, "Could not connect to server at {host}:{port}. Reason: {reason}.", result.Host, result.Port, result.FailureReason);
    }

    Meter CreateMeter() => new(MeterName, MeterVersion);
}
