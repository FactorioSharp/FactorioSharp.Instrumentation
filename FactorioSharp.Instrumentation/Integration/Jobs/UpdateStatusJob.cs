using FactorioSharp.Instrumentation.Integration.Model;
using FactorioSharp.Instrumentation.Scheduling.Jobs;
using FactorioSharp.Rcon;

namespace FactorioSharp.Instrumentation.Integration.Jobs;

class UpdateStatusJob : Job
{
    public override Task OnConnectAsync(FactorioRconClient client, FactorioServerData data, CancellationToken cancellationToken)
    {
        data.Status = true;
        return Task.CompletedTask;
    }

    public override Task OnDisconnectAsync(FactorioServerData data, CancellationToken cancellationToken)
    {
        data.Status = false;
        return Task.CompletedTask;
    }
}
