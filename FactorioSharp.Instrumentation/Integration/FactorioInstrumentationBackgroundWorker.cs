using System.Diagnostics.Metrics;
using System.Reflection;
using FactorioSharp.Instrumentation.Integration.Jobs;
using FactorioSharp.Instrumentation.Integration.Model;
using FactorioSharp.Instrumentation.Meters;
using FactorioSharp.Instrumentation.Scheduling.Jobs;
using FactorioSharp.Rcon;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FactorioSharp.Instrumentation.Integration;

/// <summary>
///     Collect data from the factorio server periodically and emit it on the available meters
/// </summary>
class FactorioInstrumentationBackgroundWorker : BackgroundService
{
    readonly string _host;
    readonly int _port;
    readonly string _password;
    readonly FactorioInstrumentationOptions _options;
    readonly ILoggerFactory _loggerFactory;
    readonly ILogger<FactorioInstrumentationBackgroundWorker> _logger;
    FactorioRconClient? _client;
    readonly FactorioServerData _cache;
    readonly List<Job> _jobs;

    public Meter Meter { get; }

    public FactorioInstrumentationBackgroundWorker(string host, int port, string password, FactorioInstrumentationOptions options, ILoggerFactory loggerFactory)
    {
        _host = host;
        _port = port;
        _password = password;
        _options = options;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<FactorioInstrumentationBackgroundWorker>();

        _cache = new FactorioServerData();

        AssemblyName assemblyName = typeof(FactorioInstruments).Assembly.GetName();
        Meter = new Meter(assemblyName.Name!, assemblyName.Version?.ToString());

        FactorioInstruments.Setup(Meter, _cache, options);

        _jobs = new List<Job> { new UpdateStatusJob(), new UpdateItemsJob(), new UpdateFluidsJob() };
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
        return base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        TimeSpan minDelayBetweenObservations = TimeSpan.FromSeconds(1);
        TimeSpan minDelayBetweenConnectionAttempts = TimeSpan.FromSeconds(10);

        await ExecuteOnStartAsync(stoppingToken);

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
                    await ExecuteOnConnectAsync(getClientResult.Client!, stoppingToken);
                }

                await ExecuteOnTickAsync(getClientResult.Client!, stoppingToken);

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
                    await ExecuteOnDisconnectAsync(stoppingToken);
                }

                _logger.LogError(getClientResult.Exception, "Could not connect to server at {host}:{port}. Reason: {reason}.", _host, _port, getClientResult.FailureReason);

                await Task.Delay(minDelayBetweenConnectionAttempts, stoppingToken);
            }
        }

        await ExecuteOnStopAsync(stoppingToken);
    }

    async Task ExecuteOnStartAsync(CancellationToken stoppingToken)
    {
        foreach (Job job in _jobs)
        {
            await job.OnStartAsync(_cache, stoppingToken);
        }
    }

    async Task ExecuteOnConnectAsync(FactorioRconClient client, CancellationToken stoppingToken)
    {
        foreach (Job job in _jobs)
        {
            await job.OnConnectAsync(client, _cache, stoppingToken);
        }
    }

    async Task ExecuteOnTickAsync(FactorioRconClient client, CancellationToken stoppingToken)
    {
        foreach (Job job in _jobs)
        {
            await job.OnTickAsync(client, _cache, stoppingToken);
        }
    }

    async Task ExecuteOnDisconnectAsync(CancellationToken stoppingToken)
    {
        foreach (Job job in _jobs)
        {
            await job.OnDisconnectAsync(_cache, stoppingToken);
        }
    }

    async Task ExecuteOnStopAsync(CancellationToken stoppingToken)
    {
        foreach (Job job in _jobs)
        {
            await job.OnStopAsync(_cache, stoppingToken);
        }
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
