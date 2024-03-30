using System.Linq.Expressions;
using FactorioSharp.Instrumentation.Extensions;
using FactorioSharp.Rcon;
using FactorioSharp.Rcon.Model;

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
    readonly string _host;
    readonly int _port;
    readonly string _password;

    /// <summary>
    ///     Ensure that there is at most one connection attempt being performed simultaneously on the <see cref="_cachedClient" />.
    /// </summary>
    readonly object _clientConnectionLock = new();

    public FactorioClient(string host, int port, string password)
    {
        _host = host;
        _port = port;
        _password = password;
    }

    public async Task<T> ReadAsync<T>(Expression<Func<FactorioRconGlobals, T>> command) => await GetConnectedClient().ReadAsync(command);

    public async Task ExecuteAsync(Expression<Action<FactorioRconGlobals>> command) => await GetConnectedClient().ExecuteAsync(command);

    public void Disconnect() => _cachedClient?.Disconnect();

    public FactorioRconClient GetConnectedClient()
    {
        // fast exit
        if (_cachedClient is { Connected: true })
        {
            return _cachedClient;
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

            if (!_cachedClient.ConnectAsync(_password).RunSync())
            {
                throw new InvalidOperationException("Could not connect to server");
            }

            return _cachedClient;
        }
    }
}
