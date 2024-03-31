using FactorioSharp.Instrumentation.Integration.Model;
using FactorioSharp.Rcon.Model.Anonymous;

namespace FactorioSharp.Instrumentation.Integration.Jobs;

class UpdateItemsJob : ObserverJob
{
    readonly string _force;

    public UpdateItemsJob(string force)
    {
        _force = force;
    }

    public override async Task ExecuteAsync(FactorioClient client, FactorioServerData data)
    {
        FactorioFlowData<ulong> flowData = data.Forces[_force].Production.Item;

        Dictionary<string, Union1887875776> itemInputStatistics = await client.ReadAsync(g => g.Game.Forces[_force].ItemProductionStatistics.InputCounts);
        foreach (KeyValuePair<string, Union1887875776> entry in itemInputStatistics)
        {
            flowData.Inputs[entry.Key] = entry.Value.AsT0;
        }

        Dictionary<string, Union1887875776> itemOutputStatistics = await client.ReadAsync(g => g.Game.Forces[_force].ItemProductionStatistics.OutputCounts);
        foreach (KeyValuePair<string, Union1887875776> entry in itemOutputStatistics)
        {
            flowData.Outputs[entry.Key] = entry.Value.AsT0;
        }
    }
}
