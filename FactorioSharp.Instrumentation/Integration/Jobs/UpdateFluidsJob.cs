using FactorioSharp.Instrumentation.Meters;
using FactorioSharp.Instrumentation.Model;
using FactorioSharp.Instrumentation.Scheduling;
using FactorioSharp.Rcon;
using FactorioSharp.Rcon.Model.Anonymous;
using Microsoft.Extensions.Logging;

namespace FactorioSharp.Instrumentation.Integration.Jobs;

class UpdateFluidsJob : Job
{
    readonly ILogger<UpdateFluidsJob> _logger;

    public UpdateFluidsJob(ILogger<UpdateFluidsJob> logger)
    {
        _logger = logger;
    }

    public override async Task OnTickAsync(FactorioRconClient client, FactorioGameData data, FactorioMeterOptionsInternal options, CancellationToken cancellationToken)
    {
        foreach (string? force in options.MeasuredForces)
        {
            if (!data.Forces.TryGetValue(force, out FactorioForceData? forceData))
            {
                _logger.LogWarning("Data for force {force} not found in data", force);
                continue;
            }

            FactorioFlowData<double> flowData = forceData.Production.Fluid;

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            Dictionary<string, Union1732410965> fluidInputStatistics = await client.ReadAsync((g, f) => g.Game.Forces[f].FluidProductionStatistics.InputCounts, force);
            foreach (KeyValuePair<string, Union1732410965> entry in fluidInputStatistics)
            {
                flowData.Inputs[entry.Key] = entry.Value.AsT1;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            Dictionary<string, Union1732410965> fluidOutputStatistics = await client.ReadAsync((g, f) => g.Game.Forces[f].FluidProductionStatistics.OutputCounts, force);
            foreach (KeyValuePair<string, Union1732410965> entry in fluidOutputStatistics)
            {
                flowData.Outputs[entry.Key] = entry.Value.AsT1;
            }
        }
    }
}
