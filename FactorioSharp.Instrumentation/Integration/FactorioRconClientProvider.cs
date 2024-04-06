using FactorioSharp.Rcon;
using Microsoft.Extensions.Logging;

namespace FactorioSharp.Instrumentation.Integration;

class FactorioRconClientProvider : IDisposable
{
    FactorioRconClient? _client;
    readonly FactorioServerOptions _options;
    readonly ILogger<FactorioRconClientProvider> _logger;

    public FactorioRconClientProvider(FactorioServerOptions options, ILogger<FactorioRconClientProvider> logger)
    {
        _options = options;
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
        if (_options.Uri == null || _options.RconPassword == null)
        {
            throw new InvalidOperationException("Could not determine server URI or RCON password");
        }

        try
        {
            if (_client is { Connected: true })
            {
                return GetConnectedClientResult.Success(_options.Uri, _client);
            }

            if (_client != null)
            {
                _logger.LogDebug("Connection to {uri} has been lost, reconnection attempt...", _options.Uri);
                _client.Dispose();
            }
            else
            {
                _logger.LogDebug("Connection attempt to {uri}...", _options.Uri);
            }

            _client = new FactorioRconClient(_options.Uri.Host, _options.Uri.Port) { Silent = _options.SilentCommands };


            if (await _client.ConnectAsync(_options.RconPassword))
            {
                _logger.LogDebug("Connected to {uri}.", _options.Uri);
                return GetConnectedClientResult.Success(_options.Uri, _client);
            }

            _logger.LogDebug("Connection to {uri} failed.", _options.Uri);

            _client.Dispose();
            _client = null;
            return GetConnectedClientResult.Failure(_options.Uri, $"Connection or authentication to {_options.Uri} failed, double check the host, port and password.");
        }
        catch (Exception exn)
        {
            return GetConnectedClientResult.Failure(_options.Uri, exn);
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
