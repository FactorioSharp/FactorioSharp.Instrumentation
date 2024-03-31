using FactorioSharp.Instrumentation.Integration.Model;

namespace FactorioSharp.Instrumentation.Integration.Jobs;

/// <summary>
///     Observe data from the factorio server
/// </summary>
abstract class ObserverJob
{
    public abstract Task ExecuteAsync(FactorioClient client, FactorioServerData data);
}
