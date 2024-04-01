using FactorioSharp.Instrumentation.Meters;
using FactorioSharp.Instrumentation.Model;
using FactorioSharp.Instrumentation.Scheduling;
using FactorioSharp.Rcon;
using FactorioSharp.Rcon.Model.Anonymous;
using Microsoft.Extensions.Logging;

namespace FactorioSharp.Instrumentation.Integration.Jobs;

class UpdateItemsJob : Job
{
    readonly ILogger<UpdateItemsJob> _logger;

    public UpdateItemsJob(ILogger<UpdateItemsJob> logger)
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

            FactorioFlowData<ulong> flowData = forceData.Production.Item;

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            Dictionary<string, Union1732410965> itemInputStatistics = await client.ReadAsync((g, f) => g.Game.Forces[f].ItemProductionStatistics.InputCounts, force);
            foreach (KeyValuePair<string, Union1732410965> entry in itemInputStatistics)
            {
                flowData.Inputs[entry.Key] = entry.Value.AsT0;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            Dictionary<string, Union1732410965> itemOutputStatistics = await client.ReadAsync((g, f) => g.Game.Forces[f].ItemProductionStatistics.OutputCounts, force);
            foreach (KeyValuePair<string, Union1732410965> entry in itemOutputStatistics)
            {
                flowData.Outputs[entry.Key] = entry.Value.AsT0;
            }
        }
    }
}
