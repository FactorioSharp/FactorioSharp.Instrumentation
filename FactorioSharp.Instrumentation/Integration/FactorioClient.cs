using System.Linq.Expressions;
using FactorioSharp.Instrumentation.Extensions;
using FactorioSharp.Rcon;
using FactorioSharp.Rcon.Model;
using Microsoft.Extensions.Logging;

namespace FactorioSharp.Instrumentation.Integration;

/// <summary>
///     Factorio RCON client with additional features:
///     <list type="bullet">
///         <item>Automatic reconnect: if the client is not connected, a connection attempt will be made before trying to execute the command (thread-safe)</item>
///     </list>
/// </summary>
/// <remarks>
///     Use the <see cref="FactorioRconClient" /> internally.
/// </remarks>
class FactorioClient
{
    FactorioRconClient? _cachedClient;
    DateTime _lastConnectionAttemptDate = DateTime.MinValue;
    readonly string _host;
    readonly int _port;
    readonly string _password;
    readonly ILogger<FactorioRconClient>? _logger;

    /// <summary>
    ///     Ensure that there is at most one connection attempt being performed simultaneously on the <see cref="_cachedClient" />.
    /// </summary>
    readonly object _clientConnectionLock = new();

    public FactorioClient(string host, int port, string password, ILogger<FactorioRconClient>? logger = null)
    {
        _host = host;
        _port = port;
        _password = password;
        _logger = logger;

        _cachedClient = GetConnectedClient();
    }

    public bool Connected => _cachedClient is { Connected: true };

    /// <summary>
    ///     The minimum amount of time to wait for before trying to reconnect again when it fails
    /// </summary>
    public TimeSpan MinReconnectionDelay { get; set; } = TimeSpan.FromSeconds(1);

    public async Task<T> ReadAsync<T>(Expression<Func<FactorioRconGlobals, T>> command) => await RequireConnectedClient().ReadAsync(command);

    public async Task ExecuteAsync(Expression<Action<FactorioRconGlobals>> command) => await RequireConnectedClient().ExecuteAsync(command);

    public void Disconnect() => _cachedClient?.Disconnect();

    FactorioRconClient RequireConnectedClient() => GetConnectedClient() ?? throw new InvalidOperationException("Could not connect to server");

    FactorioRconClient? GetConnectedClient()
    {
        // fast exit
        if (_cachedClient is { Connected: true })
        {
            return _cachedClient;
        }

        TimeSpan timeSinceLastConnectionAttempt = _lastConnectionAttemptDate - DateTime.Now;
        TimeSpan timeUntilNextConnectionAttempt = timeSinceLastConnectionAttempt - MinReconnectionDelay;
        if (timeUntilNextConnectionAttempt > TimeSpan.Zero)
        {
            return null;
        }

        lock (_clientConnectionLock)
        {
            // ensure no connection attempt has succeeded during the wait
            if (_cachedClient is { Connected: true })
            {
                return _cachedClient;
            }

            // For some reason, when the initial connection to the server fails, the same client cannot be reused to perform new connection attempts.
            // Instead the client is rebuilt each time the connection fails or is aborted.
            // This seems to be due to the underlying RconSharp library.

            _cachedClient?.Dispose();
            _cachedClient = new FactorioRconClient(_host, _port);

            _lastConnectionAttemptDate = DateTime.Now;

            bool connected = false;

            try
            {
                connected = _cachedClient.ConnectAsync(_password).RunSync();
            }
            catch (Exception exn)
            {
                _logger?.LogError(exn, "An error occurred while trying to connect to {host}:{port}.", _host, _port);
            }

            if (connected)
            {
                return _cachedClient;
            }

            _cachedClient.Dispose();
            _cachedClient = null;
            return null;
        }
    }
}
