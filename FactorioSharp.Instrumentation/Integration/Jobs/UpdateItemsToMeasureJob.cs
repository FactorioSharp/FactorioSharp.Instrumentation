using FactorioSharp.Instrumentation.Meters;
using FactorioSharp.Instrumentation.Model;
using FactorioSharp.Instrumentation.Scheduling;
using FactorioSharp.Rcon;
using FactorioSharp.Rcon.Model.Builtins;
using FactorioSharp.Rcon.Model.Classes;
using Microsoft.Extensions.Logging;

namespace FactorioSharp.Instrumentation.Integration.Jobs;

class UpdateItemsToMeasureJob : Job
{
    readonly ILogger<UpdateItemsToMeasureJob> _logger;

    public UpdateItemsToMeasureJob(ILogger<UpdateItemsToMeasureJob> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectAsync(FactorioRconClient client, FactorioGameData data, FactorioMeterOptionsInternal options, CancellationToken cancellationToken)
    {
        LuaCustomTable<string, LuaItemPrototype> itemPrototypesTable = await client.ReadAsync(g => g.Game.ItemPrototypes);
        IEnumerable<string> itemPrototypes = itemPrototypesTable.Keys;
        options.MeasuredItems = options.Original.MeasureAllItems ? itemPrototypes.ToArray() : itemPrototypes.Intersect(options.Original.MeasuredItems).ToArray();

        _logger.LogInformation("Measured items: {items}", string.Join(", ", options.MeasuredItems));
    }
}
