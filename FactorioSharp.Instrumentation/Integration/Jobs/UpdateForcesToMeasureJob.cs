﻿using FactorioSharp.Instrumentation.Meters;
using FactorioSharp.Instrumentation.Model;
using FactorioSharp.Instrumentation.Scheduling;
using FactorioSharp.Rcon;
using FactorioSharp.Rcon.Model.Anonymous;
using FactorioSharp.Rcon.Model.Builtins;
using FactorioSharp.Rcon.Model.Classes;
using Microsoft.Extensions.Logging;

namespace FactorioSharp.Instrumentation.Integration.Jobs;

class UpdateForcesToMeasureJob : Job
{
    readonly ILogger<UpdateForcesToMeasureJob> _logger;

    public UpdateForcesToMeasureJob(ILogger<UpdateForcesToMeasureJob> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectAsync(FactorioRconClient client, FactorioData data, FactorioMeterOptionsInternal options, CancellationToken _)
    {
        LuaCustomTable<Union2142551273, LuaForce> forcePrototypeTypes = await client.ReadAsync(g => g.Game.Forces);
        IEnumerable<string> forcePrototypes = forcePrototypeTypes.Keys.Where(k => k.IsT1).Select(k => k.AsT1);
        options.MeasuredForces = options.Original.MeasureAllForces ? forcePrototypes.ToArray() : forcePrototypes.Intersect(options.Original.MeasuredForces).ToArray();

        _logger.LogInformation("Measured forces: {forces}", string.Join(", ", options.MeasuredForces));

        foreach (string? force in options.MeasuredForces)
        {
            if (!data.Game.Forces.ContainsKey(force))
            {
                data.Game.Forces[force] = new FactorioForceData();
            }
        }
    }
}
