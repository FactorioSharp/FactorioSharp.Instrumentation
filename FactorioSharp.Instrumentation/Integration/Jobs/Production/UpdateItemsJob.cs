using FactorioSharp.Instrumentation.Meters;
using FactorioSharp.Instrumentation.Model;
using FactorioSharp.Instrumentation.Scheduling;
using FactorioSharp.Rcon;
using FactorioSharp.Rcon.Model.Anonymous;
using FactorioSharp.Rcon.Model.Builtins;
using FactorioSharp.Rcon.Model.Classes;
using Microsoft.Extensions.Logging;

namespace FactorioSharp.Instrumentation.Integration.Jobs.Production;

class UpdateItemsJob : Job
{
    readonly ILogger<UpdateItemsJob> _logger;

    public UpdateItemsJob(ILogger<UpdateItemsJob> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectAsync(FactorioRconClient client, FactorioData _, FactorioMeasurementOptionsInternal options, CancellationToken __)
    {
        LuaCustomTable<string, LuaItemPrototype>? itemPrototypesTable = await client.ReadAsync(g => g.Game.ItemPrototypes);
        IEnumerable<string> itemPrototypes = itemPrototypesTable?.Keys ?? [];
        options.MeasuredItems = options.Original.MeasureAllItems ? itemPrototypes.ToArray() : itemPrototypes.Intersect(options.Original.MeasuredItems).ToArray();

        _logger.LogInformation("Items: {items}", string.Join(", ", options.MeasuredItems));
    }

    public override async Task OnTickAsync(FactorioRconClient client, FactorioData data, FactorioMeasurementOptionsInternal options, CancellationToken cancellationToken)
    {
        foreach (string? force in options.MeasuredForces)
        {
            if (!data.Game.Forces.TryGetValue(force, out FactorioForceData? forceData))
            {
                _logger.LogWarning("Data for force {force} not found in data", force);
                continue;
            }

            FactorioFlowData<ulong> flowData = forceData.Production.Item;

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            Dictionary<string, Union1104138130> itemInputStatistics = await client.ReadAsync((g, f) => g.Game.Forces[f].ItemProductionStatistics.InputCounts, force)
                                                                      ?? new Dictionary<string, Union1104138130>();
            foreach (KeyValuePair<string, Union1104138130> entry in itemInputStatistics)
            {
                flowData.Inputs[entry.Key] = entry.Value.AsT0;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            Dictionary<string, Union1104138130> itemOutputStatistics = await client.ReadAsync((g, f) => g.Game.Forces[f].ItemProductionStatistics.OutputCounts, force)
                                                                       ?? new Dictionary<string, Union1104138130>();
            foreach (KeyValuePair<string, Union1104138130> entry in itemOutputStatistics)
            {
                flowData.Outputs[entry.Key] = entry.Value.AsT0;
            }
        }
    }
}
