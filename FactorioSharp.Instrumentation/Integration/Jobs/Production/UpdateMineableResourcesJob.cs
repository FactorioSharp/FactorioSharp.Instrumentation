using FactorioSharp.Instrumentation.Meters;
using FactorioSharp.Instrumentation.Model;
using FactorioSharp.Instrumentation.Scheduling;
using FactorioSharp.Rcon;
using FactorioSharp.Rcon.Model.Builtins;
using FactorioSharp.Rcon.Model.Classes;
using Microsoft.Extensions.Logging;

namespace FactorioSharp.Instrumentation.Integration.Jobs.Production;

class UpdateMineableResourcesJob : Job
{
    readonly ILogger<UpdateMineableResourcesJob> _logger;

    public UpdateMineableResourcesJob(ILogger<UpdateMineableResourcesJob> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectAsync(FactorioRconClient client, FactorioData data, FactorioMeterOptionsInternal options, CancellationToken cancellationToken)
    {
        LuaCustomTable<string, LuaEntityPrototype>? entitiesTable = await client.ReadAsync(g => g.Game.EntityPrototypes);
        IEnumerable<string> entities = entitiesTable?.Keys ?? [];

        List<MineableResource> mineableResources = [];

        foreach (string entity in entities)
        {
            string? type = await client.ReadAsync((g, e) => g.Game.EntityPrototypes[e].Type, entity);
            if (type != "resource")
            {
                continue;
            }

            bool isMineable = await client.ReadAsync((g, e) => g.Game.EntityPrototypes[e].MineableProperties.Minable, entity);
            if (!isMineable)
            {
                continue;
            }

            string? category = await client.ReadAsync((g, e) => g.Game.EntityPrototypes[e].ResourceCategory, entity);
            mineableResources.Add(new MineableResource(entity, category));
        }

        data.Game.MineableResources = mineableResources.ToArray();

        _logger.LogInformation("Mineable resources: {resources}", string.Join(", ", mineableResources.Select(r => $"{r.Name} ({r.Category})")));
    }

    public override async Task OnTickAsync(FactorioRconClient client, FactorioData data, FactorioMeterOptionsInternal options, CancellationToken cancellationToken)
    {
        foreach (string? surface in options.MeasuredSurfaces)
        {
            if (!data.Game.Surfaces.TryGetValue(surface, out FactorioSurfaceData? surfaceData))
            {
                _logger.LogWarning("Data for surface {surface} not found in data", surface);
                continue;
            }

            Dictionary<string, uint>? resources = await client.ReadAsync((g, s) => g.Game.Surfaces[s].GetResourceCounts(), surface);
            if (resources == null)
            {
                continue;
            }

            surfaceData.Resources = resources;
        }
    }
}
