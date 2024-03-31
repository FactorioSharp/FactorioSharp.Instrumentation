using System.Diagnostics.Metrics;
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

    readonly string _host;
    readonly int _port;
    readonly string _password;
    readonly FactorioMeterOptionsInternal _options;
    readonly ILoggerFactory _loggerFactory;
    readonly ILogger<FactorioInstrumentationBackgroundWorker> _logger;
    FactorioRconClient? _client;
    readonly FactorioServerData _cache;
    readonly List<Job> _jobs;
    Meter? _meter;

    public FactorioInstrumentationBackgroundWorker(string host, int port, string password, IOptions<FactorioMeterOptions> options, ILoggerFactory loggerFactory)
    {
        _host = host;
        _port = port;
        _password = password;
        _options = new FactorioMeterOptionsInternal(options.Value);
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<FactorioInstrumentationBackgroundWorker>();

        _cache = new FactorioServerData();
        _meter = CreateMeter();

        _jobs = new List<Job>
        {
            new UpdateForcesToMeasureJob(_loggerFactory.CreateLogger<UpdateForcesToMeasureJob>()),
            new UpdateItemsToMeasureJob(_loggerFactory.CreateLogger<UpdateItemsToMeasureJob>()),
            new UpdateFluidsToMeasureJob(_loggerFactory.CreateLogger<UpdateFluidsToMeasureJob>()),
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
        _client?.Dispose();
        _meter?.Dispose();
        return base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        TimeSpan minDelayBetweenObservations = TimeSpan.FromSeconds(1);
        TimeSpan minDelayBetweenConnectionAttempts = TimeSpan.FromSeconds(10);

        await ExecuteOnStartAsync(_options, stoppingToken);

        bool isConnected = false;

        while (!stoppingToken.IsCancellationRequested)
        {
            DateTime startTime = DateTime.Now;

            GetConnectedClientResult getClientResult = await TryGetConnectedClient();
            if (getClientResult.Succeeded)
            {
                if (!isConnected)
                {
                    isConnected = true;
                    await ExecuteOnConnectAsync(getClientResult.Client!, _options, stoppingToken);

                    _meter ??= CreateMeter();
                    FactorioInstruments.Setup(_meter, _cache, _options);
                }

                await ExecuteOnTickAsync(getClientResult.Client!, _options, stoppingToken);

                TimeSpan elapsed = DateTime.Now - startTime;
                TimeSpan toWait = minDelayBetweenObservations - elapsed;

                if (toWait > TimeSpan.Zero)
                {
                    await Task.Delay(toWait, stoppingToken);
                }
            }
            else
            {
                if (isConnected)
                {
                    isConnected = false;
                    await ExecuteOnDisconnectAsync(_options, stoppingToken);

                    _meter?.Dispose();
                    _meter = CreateMeter();
                }

                _logger.LogError(getClientResult.Exception, "Could not connect to server at {host}:{port}. Reason: {reason}.", _host, _port, getClientResult.FailureReason);

                await Task.Delay(minDelayBetweenConnectionAttempts, stoppingToken);
            }
        }

        await ExecuteOnStopAsync(_options, stoppingToken);
    }

    async Task ExecuteOnStartAsync(FactorioMeterOptionsInternal options, CancellationToken stoppingToken)
    {
        foreach (Job job in _jobs)
        {
            await job.OnStartAsync(_cache, options, stoppingToken);
        }
    }

    async Task ExecuteOnConnectAsync(FactorioRconClient client, FactorioMeterOptionsInternal options, CancellationToken stoppingToken)
    {
        foreach (Job job in _jobs)
        {
            await job.OnConnectAsync(client, _cache, options, stoppingToken);
        }
    }

    async Task ExecuteOnTickAsync(FactorioRconClient client, FactorioMeterOptionsInternal options, CancellationToken stoppingToken)
    {
        foreach (Job job in _jobs)
        {
            await job.OnTickAsync(client, _cache, options, stoppingToken);
        }
    }

    async Task ExecuteOnDisconnectAsync(FactorioMeterOptionsInternal options, CancellationToken stoppingToken)
    {
        foreach (Job job in _jobs)
        {
            await job.OnDisconnectAsync(_cache, options, stoppingToken);
        }
    }

    async Task ExecuteOnStopAsync(FactorioMeterOptionsInternal options, CancellationToken stoppingToken)
    {
        foreach (Job job in _jobs)
        {
            await job.OnStopAsync(_cache, options, stoppingToken);
        }
    }

    Meter CreateMeter()
    {
        Meter meter = new(MeterName, MeterVersion);
        meter.CreateObservableGauge("factorio.server.status", () => _client is { Connected: true } ? 1 : 0, "1", "The current status of the factorio server");

        return meter;
    }

    async Task<GetConnectedClientResult> TryGetConnectedClient()
    {
        try
        {
            if (_client is { Connected: true })
            {
                return GetConnectedClientResult.Success(_client);
            }

            if (_client != null)
            {
                _logger.LogDebug("Connection to {host}:{port} has been lost, reconnection attempt...", _host, _port);
                _client.Dispose();
            }
            else
            {
                _logger.LogDebug("Connection attempt to {host}:{port}...", _host, _port);
            }

            _client = new FactorioRconClient(_host, _port);


            if (await _client.ConnectAsync(_password))
            {
                _logger.LogDebug("Connected to {host}:{port}.", _host, _port);
                return GetConnectedClientResult.Success(_client);
            }

            _logger.LogDebug("Connection to {host}:{port} failed.", _host, _port);

            _client.Dispose();
            _client = null;
            return GetConnectedClientResult.Failure($"Connection or authentication to {_host}:{_port} failed, double check the host, port and password.");
        }
        catch (Exception exn)
        {
            return GetConnectedClientResult.Failure(exn);
        }
    }

    class GetConnectedClientResult
    {
        public bool Succeeded => Client != null;
        public FactorioRconClient? Client { get; }
        public string? FailureReason { get; }
        public Exception? Exception { get; }

        GetConnectedClientResult(FactorioRconClient? client, string? failureReason, Exception? exception)
        {
            Client = client;
            FailureReason = failureReason;
            Exception = exception;
        }

        public static GetConnectedClientResult Success(FactorioRconClient client) => new(client, null, null);
        public static GetConnectedClientResult Failure(Exception exception) => new(null, exception.Message, exception);
        public static GetConnectedClientResult Failure(string reason) => new(null, reason, null);
    }
}
