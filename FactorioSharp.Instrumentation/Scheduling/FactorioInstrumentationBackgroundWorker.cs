using System.Diagnostics.Metrics;
using FactorioSharp.Instrumentation.Integration;
using FactorioSharp.Instrumentation.Integration.Jobs.Game;
using FactorioSharp.Instrumentation.Integration.Jobs.Production;
using FactorioSharp.Instrumentation.Integration.Jobs.Server;
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
    readonly FactorioMeasurementOptionsInternal _options;
    readonly ILogger<FactorioInstrumentationBackgroundWorker> _logger;
    readonly FactorioData _data;
    readonly JobCollection _jobs;
    Meter? _meter;

    public FactorioInstrumentationBackgroundWorker(IOptions<FactorioInstrumentationOptions> options, ILoggerFactory loggerFactory)
    {
        _clientProvider = new FactorioRconClientProvider(options.Value.Server, loggerFactory.CreateLogger<FactorioRconClientProvider>());
        _options = new FactorioMeasurementOptionsInternal(options.Value.Measurement);
        _logger = loggerFactory.CreateLogger<FactorioInstrumentationBackgroundWorker>();

        _data = new FactorioData(options.Value.Server.Uri ?? throw new ArgumentNullException(nameof(options.Value.Server.Uri)), options.Value.Server.Name);

        _meter = CreateMeter();
        FactorioServerInstruments.Setup(_meter, _data.Server, _options);

        _jobs = new JobCollection(loggerFactory.CreateLogger<JobCollection>())
        {
            new UpdateServerStatusJob(loggerFactory.CreateLogger<UpdateServerStatusJob>()),
            new UpdateServerModsJob(loggerFactory.CreateLogger<UpdateServerModsJob>()),
            new UpdateServerPlayersJob(loggerFactory.CreateLogger<UpdateServerPlayersJob>()),
            new UpdateGameJob(loggerFactory.CreateLogger<UpdateGameJob>()),
            new UpdateSurfacesJob(loggerFactory.CreateLogger<UpdateSurfacesJob>()),
            new UpdateForcesJob(loggerFactory.CreateLogger<UpdateForcesJob>()),
            new UpdateItemsJob(loggerFactory.CreateLogger<UpdateItemsJob>()),
            new UpdateFluidsJob(loggerFactory.CreateLogger<UpdateFluidsJob>()),
            new UpdateMineableResourcesJob(loggerFactory.CreateLogger<UpdateMineableResourcesJob>()),
            new UpdatePowerJob(loggerFactory.CreateLogger<UpdatePowerJob>())
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
        await _jobs.ExecuteOnStartAsync(_data, _options, stoppingToken);

        _isConnected = false;

        while (!stoppingToken.IsCancellationRequested)
        {
            FactorioRconClientProvider.GetConnectedClientResult result = await _clientProvider.TryGetConnectedClient();
            if (result.Succeeded)
            {
                if (!_isConnected)
                {
                    _logger.LogInformation("Connection with server acquired, reading initial data...");

                    _isConnected = true;

                    await _jobs.ExecuteOnConnectAsync(_data, result.Client!, _options, stoppingToken);

                    if (_meter == null)
                    {
                        _meter = CreateMeter();
                        FactorioServerInstruments.Setup(_meter, _data.Server, _options);
                    }

                    FactorioGameInstruments.Setup(_meter, _data);

                    _logger.LogInformation("Initialization done.");
                }

                DateTime startTime = DateTime.Now;

                if (_options.Original.SpreadMeasurements)
                {
                    await SpreadTicksOverOnePeriod(result.Client!, stoppingToken);
                }
                else
                {
                    await _jobs.ExecuteOnTickAsync(_data, result.Client!, _options, stoppingToken);
                }

                TimeSpan elapsed = DateTime.Now - startTime;
                TimeSpan toWait = _options.Original.ObservationInterval - elapsed;

                if (toWait > TimeSpan.Zero)
                {
                    await Task.Delay(toWait, stoppingToken);
                }
            }
            else
            {
                if (_isConnected)
                {
                    _logger.LogInformation("Connection with server lost.");

                    _isConnected = false;

                    await _jobs.ExecuteOnDisconnectAsync(_data, _options, stoppingToken);

                    _meter?.Dispose();
                    _meter = CreateMeter();
                    FactorioServerInstruments.Setup(_meter, _data.Server, _options);
                }

                _logger.LogError(result.Exception, "Could not connect to server at {host}:{port}. Reason: {reason}.", result.Uri.Host, result.Uri.Port, result.FailureReason);
                await Task.Delay(_options.Original.ReconnectionInterval, stoppingToken);
            }
        }

        await _jobs.ExecuteOnStopAsync(_data, _options, stoppingToken);
    }

    async Task SpreadTicksOverOnePeriod(FactorioRconClient client, CancellationToken cancellationToken)
    {
        List<Task> tasks = [];
        TimeSpan baseOffset = _options.Original.ObservationInterval / _jobs.Count;
        for (int i = 0; i < _jobs.Count; i++)
        {
            tasks.Add(OffsetTick(i));

            continue;

            async Task OffsetTick(int index)
            {
                TimeSpan offset = baseOffset * index;
                await Task.Delay(offset, cancellationToken);
                await _jobs.ExecuteOnTickAsync(index, _data, client, _options, cancellationToken);
            }
        }

        await Task.WhenAll(tasks);
    }

    static Meter CreateMeter() => new(FactorioInstrumentationConstants.MeterName, FactorioInstrumentationConstants.MeterVersion);
}
