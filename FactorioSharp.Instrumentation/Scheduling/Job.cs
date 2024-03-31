using FactorioSharp.Instrumentation.Meters;
using FactorioSharp.Instrumentation.Model;
using FactorioSharp.Rcon;

namespace FactorioSharp.Instrumentation.Scheduling;

/// <summary>
///     Long-lived object with hooks that are called during the life of the application.
/// </summary>
abstract class Job
{
    /// <summary>
    ///     Method called at the beginning of the main loop
    /// </summary>
    public virtual Task OnStartAsync(FactorioServerData data, FactorioMeterOptionsInternal options, CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>
    ///     Method called when the application connects to the factorio server.
    ///     It is called once when the initial connection is acquired, and once every time the connection is lost and acquired again.
    /// </summary>
    public virtual Task OnConnectAsync(FactorioRconClient client, FactorioServerData data, FactorioMeterOptionsInternal options, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    /// <summary>
    ///     Method called periodically when the application is connected to the factorio server
    /// </summary>
    public virtual Task OnTickAsync(FactorioRconClient client, FactorioServerData data, FactorioMeterOptionsInternal options, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    /// <summary>
    ///     Method called when the connection to the factorio server is lost.
    ///     The connection can be lost because of an error or because the application is shutting down.
    /// </summary>
    public virtual Task OnDisconnectAsync(FactorioServerData data, FactorioMeterOptionsInternal options, CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>
    ///     Method called at the end of the main loop, when the application is exited gracefully. It is called after disconnecting from the factorio server.
    /// </summary>
    public virtual Task OnStopAsync(FactorioServerData data, FactorioMeterOptionsInternal options, CancellationToken cancellationToken) => Task.CompletedTask;
}
