using FactorioSharp.Instrumentation.Meters;
using FactorioSharp.Instrumentation.Model;
using FactorioSharp.Instrumentation.Scheduling;
using FactorioSharp.Rcon;
using FactorioSharp.Rcon.Model.Builtins;
using FactorioSharp.Rcon.Model.Classes;
using Microsoft.Extensions.Logging;

namespace FactorioSharp.Instrumentation.Integration.Jobs;

class UpdateFluidsToMeasureJob : Job
{
    readonly ILogger<UpdateFluidsToMeasureJob> _logger;

    public UpdateFluidsToMeasureJob(ILogger<UpdateFluidsToMeasureJob> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectAsync(FactorioRconClient client, FactorioServerData data, FactorioMeterOptionsInternal options, CancellationToken cancellationToken)
    {
        LuaCustomTable<string, LuaFluidPrototype> fluidPrototypesTable = await client.ReadAsync(g => g.Game.FluidPrototypes);
        IEnumerable<string> fluidPrototypes = fluidPrototypesTable.Keys;
        options.MeasuredFluids = options.Original.MeasureAllFluids ? fluidPrototypes.ToArray() : fluidPrototypes.Intersect(options.Original.MeasuredFluids).ToArray();

        _logger.LogInformation("Measured fluids: {fluids}", string.Join(", ", options.MeasuredFluids));
    }
}
