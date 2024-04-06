using FactorioSharp.Rcon;
using Microsoft.Extensions.Logging;

namespace FactorioSharp.Instrumentation.Integration;

class FactorioRconClientProvider : IDisposable
{
    FactorioRconClient? _client;
    readonly Uri _uri;
    readonly string _password;
    readonly ILogger<FactorioRconClientProvider> _logger;

    public FactorioRconClientProvider(Uri uri, string password, ILogger<FactorioRconClientProvider> logger)
    {
        _uri = uri;
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
                return GetConnectedClientResult.Success(_uri, _client);
            }

            if (_client != null)
            {
                _logger.LogDebug("Connection to {uri} has been lost, reconnection attempt...", _uri);
                _client.Dispose();
            }
            else
            {
                _logger.LogDebug("Connection attempt to {uri}...", _uri);
            }

            _client = new FactorioRconClient(_uri.Host, _uri.Port);


            if (await _client.ConnectAsync(_password))
            {
                _logger.LogDebug("Connected to {uri}.", _uri);
                return GetConnectedClientResult.Success(_uri, _client);
            }

            _logger.LogDebug("Connection to {uri} failed.", _uri);

            _client.Dispose();
            _client = null;
            return GetConnectedClientResult.Failure(_uri, $"Connection or authentication to {_uri} failed, double check the host, port and password.");
        }
        catch (Exception exn)
        {
            return GetConnectedClientResult.Failure(_uri, exn);
        }
    }

    public class GetConnectedClientResult
    {
        public bool Succeeded => Client != null;
        public Uri Uri { get; }
        public FactorioRconClient? Client { get; }
        public string? FailureReason { get; }
        public Exception? Exception { get; }

        GetConnectedClientResult(Uri uri, FactorioRconClient? client, string? failureReason, Exception? exception)
        {
            Uri = uri;
            Client = client;
            FailureReason = failureReason;
            Exception = exception;
        }

        public static GetConnectedClientResult Success(Uri uri, FactorioRconClient client) => new(uri, client, null, null);
        public static GetConnectedClientResult Failure(Uri uri, Exception exception) => new(uri, null, exception.Message, exception);
        public static GetConnectedClientResult Failure(Uri uri, string reason) => new(uri, null, reason, null);
    }

    public void Dispose()
    {
        _client?.Dispose();
        GC.SuppressFinalize(this);
    }
}
