using FactorioSharp.Instrumentation.Integration.Model;
using FactorioSharp.Instrumentation.Scheduling.Jobs;
using FactorioSharp.Rcon;
using FactorioSharp.Rcon.Model.Anonymous;

namespace FactorioSharp.Instrumentation.Integration.Jobs;

class UpdateItemsJob : Job
{
    public override async Task OnTickAsync(FactorioRconClient client, FactorioServerData data, CancellationToken cancellationToken)
    {
        foreach ((string force, FactorioForceData? value) in data.Forces)
        {
            FactorioFlowData<ulong> flowData = value.Production.Item;

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            Dictionary<string, Union1887875776> itemInputStatistics = await client.ReadAsync(g => g.Game.Forces[force].ItemProductionStatistics.InputCounts);
            foreach (KeyValuePair<string, Union1887875776> entry in itemInputStatistics)
            {
                flowData.Inputs[entry.Key] = entry.Value.AsT0;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            Dictionary<string, Union1887875776> itemOutputStatistics = await client.ReadAsync(g => g.Game.Forces[force].ItemProductionStatistics.OutputCounts);
            foreach (KeyValuePair<string, Union1887875776> entry in itemOutputStatistics)
            {
                flowData.Outputs[entry.Key] = entry.Value.AsT0;
            }
        }
    }
}
