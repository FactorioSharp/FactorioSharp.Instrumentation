using FactorioSharp.Instrumentation.Integration.Model;
using FactorioSharp.Rcon;
using FactorioSharp.Rcon.Model.Anonymous;

namespace FactorioSharp.Instrumentation.Integration.Jobs;

class UpdateItemsJob : ObserverJob
{
    readonly string _force;

    public UpdateItemsJob(FactorioRconClient client, FactorioServerData data, string force) : base(client, data)
    {
        _force = force;
    }

    public override async Task ExecuteAsync()
    {
        FactorioFlowData<ulong> flowData = Data.Forces[_force].Production.Item;

        Dictionary<string, Union1887875776> itemInputStatistics = await Client.ReadAsync(g => g.Game.Forces[_force].ItemProductionStatistics.InputCounts);
        foreach (KeyValuePair<string, Union1887875776> entry in itemInputStatistics)
        {
            flowData.Inputs[entry.Key] = entry.Value.AsT0;
        }

        Dictionary<string, Union1887875776> itemOutputStatistics = await Client.ReadAsync(g => g.Game.Forces[_force].ItemProductionStatistics.OutputCounts);
        foreach (KeyValuePair<string, Union1887875776> entry in itemOutputStatistics)
        {
            flowData.Outputs[entry.Key] = entry.Value.AsT0;
        }
    }
}
