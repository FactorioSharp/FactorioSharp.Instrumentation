using FactorioSharp.Instrumentation.Meters;
using FactorioSharp.Instrumentation.Model;
using FactorioSharp.Instrumentation.Scheduling;
using FactorioSharp.Rcon;
using FactorioSharp.Rcon.Model.Anonymous;

namespace FactorioSharp.Instrumentation.Integration.Jobs;

class UpdateItemsJob : Job
{
    public override async Task OnTickAsync(FactorioRconClient client, FactorioServerData data, FactorioMeterOptionsInternal options, CancellationToken cancellationToken)
    {
        foreach ((string force, FactorioForceData? value) in data.Forces)
        {
            FactorioFlowData<ulong> flowData = value.Production.Item;

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            Dictionary<string, Union1732410965> itemInputStatistics = await client.ReadAsync(g => g.Game.Forces[force].ItemProductionStatistics.InputCounts);
            foreach (KeyValuePair<string, Union1732410965> entry in itemInputStatistics)
            {
                flowData.Inputs[entry.Key] = entry.Value.AsT0;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            Dictionary<string, Union1732410965> itemOutputStatistics = await client.ReadAsync(g => g.Game.Forces[force].ItemProductionStatistics.OutputCounts);
            foreach (KeyValuePair<string, Union1732410965> entry in itemOutputStatistics)
            {
                flowData.Outputs[entry.Key] = entry.Value.AsT0;
            }
        }
    }
}
