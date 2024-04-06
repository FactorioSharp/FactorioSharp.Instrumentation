using System.Text.Json;
using FactorioSharp.Instrumentation.Meters;
using FactorioSharp.Instrumentation.Model;
using FactorioSharp.Instrumentation.Scheduling;
using FactorioSharp.Rcon;
using Microsoft.Extensions.Logging;

namespace FactorioSharp.Instrumentation.Integration.Jobs.Production;

class UpdateMineableResourcesJob : Job
{
    readonly ILogger<UpdateMineableResourcesJob> _logger;
    static readonly JsonSerializerOptions JsonSerializerOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

    public UpdateMineableResourcesJob(ILogger<UpdateMineableResourcesJob> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectAsync(FactorioRconClient client, FactorioData data, FactorioMeasurementOptionsInternal options, CancellationToken cancellationToken)
    {
        MineableResource[] resources = await GetMineableResources(client);
        data.Game.MineableResources = resources.ToDictionary(r => r.Name, r => r);

        _logger.LogInformation("Mineable resources: {resources}", string.Join(", ", resources.Select(r => $"{r.Name} ({r.Category})")));
    }

    public override async Task OnTickAsync(FactorioRconClient client, FactorioData data, FactorioMeasurementOptionsInternal options, CancellationToken cancellationToken)
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

    static async Task<MineableResource[]> GetMineableResources(FactorioRconClient client)
    {
        string result = await client.LowLevelClient.ExecuteAsync(
            """
            local result = {}

            for k, v in pairs(game.entity_prototypes) do
                if v.type == "resource" and v.mineable_properties.minable then
                    table.insert(result, { name = v.name, category = v.resource_category })
                end
            end

            rcon.print(game.table_to_json(result));
            """
        );

        return JsonSerializer.Deserialize<MineableResource[]>(result, JsonSerializerOptions) ?? Array.Empty<MineableResource>();
    }
}
