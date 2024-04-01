using FactorioSharp.Instrumentation.Meters;
using FactorioSharp.Instrumentation.Model;
using FactorioSharp.Instrumentation.Scheduling;
using FactorioSharp.Rcon;
using FactorioSharp.Rcon.Model.Anonymous;

namespace FactorioSharp.Instrumentation.Integration.Jobs;

class UpdateFluidsJob : Job
{
    public override async Task OnTickAsync(FactorioRconClient client, FactorioGameData data, FactorioMeterOptionsInternal options, CancellationToken cancellationToken)
    {
        foreach ((string force, FactorioForceData? value) in data.Forces)
        {
            FactorioFlowData<double> flowData = value.Production.Fluid;

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            Dictionary<string, Union1732410965> fluidInputStatistics = await client.ReadAsync(g => g.Game.Forces[force].FluidProductionStatistics.InputCounts);
            foreach (KeyValuePair<string, Union1732410965> entry in fluidInputStatistics)
            {
                flowData.Inputs[entry.Key] = entry.Value.AsT0;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            Dictionary<string, Union1732410965> fluidOutputStatistics = await client.ReadAsync(g => g.Game.Forces[force].FluidProductionStatistics.OutputCounts);
            foreach (KeyValuePair<string, Union1732410965> entry in fluidOutputStatistics)
            {
                flowData.Outputs[entry.Key] = entry.Value.AsT0;
            }
        }
    }
}
