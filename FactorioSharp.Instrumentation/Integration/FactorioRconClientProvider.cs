using FactorioSharp.Rcon;
using Microsoft.Extensions.Logging;

namespace FactorioSharp.Instrumentation.Integration;

public class FactorioRconClientProvider : IDisposable
{
    FactorioRconClient? _client;
    readonly string _host;
    readonly int _port;
    readonly string _password;
    readonly ILogger<FactorioRconClientProvider> _logger;

    public FactorioRconClientProvider(string host, int port, string password, ILogger<FactorioRconClientProvider> logger)
    {
        _host = host;
        _port = port;
        _password = password;
        _logger = logger;

    }

    /// <summary>
    ///     Get a <see cref="FactorioRconClient" /> connected to the server.
    /// </summary>
    /// <remarks>
    ///     Caching is managed by the factory: if a client can be reused, it will be.
    /// </remarks>
    public async Task<GetConnectedClientResult> TryGetConnectedClient()
    {
        try
        {
            if (_client is { Connected: true })
            {
                return GetConnectedClientResult.Success(_host, _port, _client);
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
                return GetConnectedClientResult.Success(_host, _port, _client);
            }

            _logger.LogDebug("Connection to {host}:{port} failed.", _host, _port);

            _client.Dispose();
            _client = null;
            return GetConnectedClientResult.Failure(_host, _port, $"Connection or authentication to {_host}:{_port} failed, double check the host, port and password.");
        }
        catch (Exception exn)
        {
            return GetConnectedClientResult.Failure(_host, _port, exn);
        }
    }

    public class GetConnectedClientResult
    {
        public bool Succeeded => Client != null;
        public string Host { get; }
        public int Port { get; }
        public FactorioRconClient? Client { get; }
        public string? FailureReason { get; }
        public Exception? Exception { get; }

        GetConnectedClientResult(string host, int port, FactorioRconClient? client, string? failureReason, Exception? exception)
        {
            Host = host;
            Port = port;
            Client = client;
            FailureReason = failureReason;
            Exception = exception;
        }

        public static GetConnectedClientResult Success(string host, int port, FactorioRconClient client) => new(host, port, client, null, null);
        public static GetConnectedClientResult Failure(string host, int port, Exception exception) => new(host, port, null, exception.Message, exception);
        public static GetConnectedClientResult Failure(string host, int port, string reason) => new(host, port, null, reason, null);
    }

    public void Dispose()
    {
        _client?.Dispose();
        GC.SuppressFinalize(this);
    }
}
