using FactorioSharp.Instrumentation.Integration.Model;
using FactorioSharp.Rcon;

namespace FactorioSharp.Instrumentation.Integration.Jobs;

/// <summary>
///     Read data from the factorio server and update the cache
/// </summary>
abstract class ObserverJob : IJob
{
    protected FactorioRconClient Client { get; }
    protected FactorioServerData Data { get; }

    protected ObserverJob(FactorioRconClient client, FactorioServerData data)
    {
        Client = client;
        Data = data;
    }

    public abstract Task ExecuteAsync();
}
