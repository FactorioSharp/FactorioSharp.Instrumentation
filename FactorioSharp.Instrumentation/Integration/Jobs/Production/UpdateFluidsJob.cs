using FactorioSharp.Instrumentation.Meters;
using FactorioSharp.Instrumentation.Model;
using FactorioSharp.Instrumentation.Scheduling;
using FactorioSharp.Rcon;
using FactorioSharp.Rcon.Model.Anonymous;
using FactorioSharp.Rcon.Model.Builtins;
using FactorioSharp.Rcon.Model.Classes;
using Microsoft.Extensions.Logging;

namespace FactorioSharp.Instrumentation.Integration.Jobs.Production;

class UpdateFluidsJob : Job
{
    readonly ILogger<UpdateFluidsJob> _logger;

    public UpdateFluidsJob(ILogger<UpdateFluidsJob> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectAsync(FactorioRconClient client, FactorioData _, FactorioMeterOptionsInternal options, CancellationToken __)
    {
        LuaCustomTable<string, LuaFluidPrototype>? fluidPrototypesTable = await client.ReadAsync(g => g.Game.FluidPrototypes);
        IEnumerable<string> fluidPrototypes = fluidPrototypesTable?.Keys ?? [];
        options.MeasuredFluids = options.Original.MeasureAllFluids ? fluidPrototypes.ToArray() : fluidPrototypes.Intersect(options.Original.MeasuredFluids).ToArray();

        _logger.LogInformation("Fluids: {fluids}", string.Join(", ", options.MeasuredFluids));
    }

    public override async Task OnTickAsync(FactorioRconClient client, FactorioData data, FactorioMeterOptionsInternal options, CancellationToken cancellationToken)
    {
        foreach (string? force in options.MeasuredForces)
        {
            if (!data.Game.Forces.TryGetValue(force, out FactorioForceData? forceData))
            {
                _logger.LogWarning("Data for force {force} not found in data", force);
                continue;
            }

            FactorioFlowData<double> flowData = forceData.Production.Fluid;

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            Dictionary<string, Union1104138130> fluidInputStatistics = await client.ReadAsync((g, f) => g.Game.Forces[f].FluidProductionStatistics.InputCounts, force)
                                                                       ?? new Dictionary<string, Union1104138130>();
            foreach (KeyValuePair<string, Union1104138130> entry in fluidInputStatistics)
            {
                flowData.Inputs[entry.Key] = entry.Value.Match(l => l, d => d);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            Dictionary<string, Union1104138130> fluidOutputStatistics = await client.ReadAsync((g, f) => g.Game.Forces[f].FluidProductionStatistics.OutputCounts, force)
                                                                        ?? new Dictionary<string, Union1104138130>();
            foreach (KeyValuePair<string, Union1104138130> entry in fluidOutputStatistics)
            {
                flowData.Outputs[entry.Key] = entry.Value.Match(l => l, d => d);
            }
        }
    }
}
