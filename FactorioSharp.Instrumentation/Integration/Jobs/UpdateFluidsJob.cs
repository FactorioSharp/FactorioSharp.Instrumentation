using FactorioSharp.Instrumentation.Integration.Model;
using FactorioSharp.Rcon.Model.Anonymous;

namespace FactorioSharp.Instrumentation.Integration.Jobs;

class UpdateFluidsJob : ObserverJob
{
    readonly string _force;

    public UpdateFluidsJob(string force)
    {
        _force = force;
    }

    public override async Task ExecuteAsync(FactorioClient client, FactorioServerData data)
    {
        FactorioFlowData<double> flowData = data.Forces[_force].Production.Fluid;

        Dictionary<string, Union1887875776> itemInputStatistics = await client.ReadAsync(g => g.Game.Forces[_force].FluidProductionStatistics.InputCounts);
        foreach (KeyValuePair<string, Union1887875776> entry in itemInputStatistics)
        {
            flowData.Inputs[entry.Key] = entry.Value.AsT1;
        }

        Dictionary<string, Union1887875776> itemOutputStatistics = await client.ReadAsync(g => g.Game.Forces[_force].FluidProductionStatistics.OutputCounts);
        foreach (KeyValuePair<string, Union1887875776> entry in itemOutputStatistics)
        {
            flowData.Outputs[entry.Key] = entry.Value.AsT1;
        }
    }
}
