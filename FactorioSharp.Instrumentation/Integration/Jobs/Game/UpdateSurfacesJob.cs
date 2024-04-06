using FactorioSharp.Instrumentation.Meters;
using FactorioSharp.Instrumentation.Model;
using FactorioSharp.Instrumentation.Scheduling;
using FactorioSharp.Rcon;
using FactorioSharp.Rcon.Model.Anonymous;
using FactorioSharp.Rcon.Model.Builtins;
using FactorioSharp.Rcon.Model.Classes;
using Microsoft.Extensions.Logging;

namespace FactorioSharp.Instrumentation.Integration.Jobs.Game;

class UpdateSurfacesJob : Job
{
    readonly ILogger<UpdateSurfacesJob> _logger;

    public UpdateSurfacesJob(ILogger<UpdateSurfacesJob> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectAsync(FactorioRconClient client, FactorioData data, FactorioMeasurementOptionsInternal options, CancellationToken cancellationToken)
    {
        LuaCustomTable<Union2142551273, LuaSurface>? surfacePrototypeTypes = await client.ReadAsync(g => g.Game.Surfaces);
        IEnumerable<string> surfacePrototypes = surfacePrototypeTypes?.Keys.Where(k => k.IsT1).Select(k => k.AsT1) ?? [];
        options.MeasuredSurfaces = options.Original.MeasureAllSurfaces ? surfacePrototypes.ToArray() : surfacePrototypes.Intersect(options.Original.MeasuredSurfaces).ToArray();

        _logger.LogInformation("Surfaces: {surfaces}", string.Join(", ", options.MeasuredSurfaces));

        foreach (string? surface in options.MeasuredSurfaces)
        {
            if (!data.Game.Surfaces.ContainsKey(surface))
            {
                data.Game.Surfaces[surface] = new FactorioSurfaceData();
            }
        }
    }
}
