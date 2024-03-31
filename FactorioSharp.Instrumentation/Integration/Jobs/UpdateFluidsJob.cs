using FactorioSharp.Instrumentation.Integration.Model;
using FactorioSharp.Instrumentation.Scheduling.Jobs;
using FactorioSharp.Rcon;
using FactorioSharp.Rcon.Model.Anonymous;

namespace FactorioSharp.Instrumentation.Integration.Jobs;

class UpdateFluidsJob : Job
{
    public override async Task OnTickAsync(FactorioRconClient client, FactorioServerData data, CancellationToken cancellationToken)
    {
        foreach ((string force, FactorioForceData? value) in data.Forces)
        {
            FactorioFlowData<double> flowData = value.Production.Fluid;

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            Dictionary<string, Union1887875776> fluidInputStatistics = await client.ReadAsync(g => g.Game.Forces[force].FluidProductionStatistics.InputCounts);
            foreach (KeyValuePair<string, Union1887875776> entry in fluidInputStatistics)
            {
                flowData.Inputs[entry.Key] = entry.Value.AsT0;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            Dictionary<string, Union1887875776> fluidOutputStatistics = await client.ReadAsync(g => g.Game.Forces[force].FluidProductionStatistics.OutputCounts);
            foreach (KeyValuePair<string, Union1887875776> entry in fluidOutputStatistics)
            {
                flowData.Outputs[entry.Key] = entry.Value.AsT0;
            }
        }
    }
}
