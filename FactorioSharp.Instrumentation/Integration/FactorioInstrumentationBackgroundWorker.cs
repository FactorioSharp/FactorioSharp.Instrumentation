using System.Diagnostics.Metrics;
using System.Reflection;
using FactorioSharp.Instrumentation.Integration.Jobs;
using FactorioSharp.Instrumentation.Integration.Model;
using FactorioSharp.Instrumentation.Meters;
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
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        TimeSpan minDelayBetweenObservations = TimeSpan.FromSeconds(1);
        TimeSpan minDelayBetweenConnectionAttempts = TimeSpan.FromSeconds(30);

        while (!stoppingToken.IsCancellationRequested)
        {
            DateTime startTime = DateTime.Now;

            GetConnectedClientResult client = await TryGetConnectedClient();
            if (client.Succeeded)
            {
                _cache.Status = true;
                await Tick(client.Client!, stoppingToken);

                TimeSpan elapsed = DateTime.Now - startTime;
                TimeSpan toWait = minDelayBetweenObservations - elapsed;

                if (toWait > TimeSpan.Zero)
                {
                    await Task.Delay(toWait, stoppingToken);
                }
            }
            else
            {
                _cache.Status = false;
                _logger.LogError(client.Exception, "Could not connect to server at {host}:{port}. Reason: {reason}.", _host, _port, client.FailureReason);

                await Task.Delay(minDelayBetweenConnectionAttempts, stoppingToken);
            }
        }
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

    async Task Tick(FactorioRconClient client, CancellationToken stoppingToken)
    {
        foreach (string force in _options.Forces)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }

            UpdateItemsJob itemsJob = new(client, _cache, force);
            await itemsJob.ExecuteAsync();

            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }

            UpdateFluidsJob fluidsJob = new(client, _cache, force);
            await fluidsJob.ExecuteAsync();
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
