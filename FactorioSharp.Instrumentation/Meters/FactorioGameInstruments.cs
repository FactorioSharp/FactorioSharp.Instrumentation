using System.Diagnostics.Metrics;
using FactorioSharp.Instrumentation.Model;

namespace FactorioSharp.Instrumentation.Meters;

static class FactorioGameInstruments
{
    public static void Setup(Meter meter, FactorioData data)
    {
        Dictionary<string, object?> tags = new();
        data.Server.EnrichTags(tags);

        SetupGameInstruments(meter, data.Game, tags);
        SetupMineableResourceInstruments(meter, data.Game, tags);
        SetupElectricNetworkInstruments(meter, data.Game, tags);
        SetupItemInstruments(meter, data.Game, tags);
        SetupFluidInstruments(meter, data.Game, tags);
    }

    static void SetupGameInstruments(Meter meter, FactorioGameData gameData, Dictionary<string, object?> tags)
    {
        meter.CreateObservableUpDownCounter("factorio.server.player.count", () => gameData.Players.Count, null, "The number of players on the factorio server", tags);
        meter.CreateObservableUpDownCounter(
            "factorio.server.connected_player.count",
            () => gameData.Players.Count(kv => kv.Value.IsOnline),
            null,
            "The number of players currently connected to the factorio server",
            tags
        );

        meter.CreateObservableCounter("factorio.game.tick", () => (long)gameData.Time.Tick, "{tick}", "The current map tick", tags);
        meter.CreateObservableCounter("factorio.game.tick_played", () => (long)gameData.Time.TicksPlayed, "{tick}", "The number of ticks since the game was created", tags);
        meter.CreateObservableGauge("factorio.game.speed", () => gameData.Time.Speed, "{tick}", "The speed to update the map at", tags);
        meter.CreateObservableUpDownCounter("factorio.game.paused", () => gameData.Time.Paused ? 1 : 0, "{tick}", "Is the game paused ?", tags);
    }

    static void SetupMineableResourceInstruments(Meter meter, FactorioGameData gameData, IDictionary<string, object?> baseTags) =>
        meter.CreateObservableUpDownCounter(
            "factorio.game.mineable_resource",
            () => gameData.Surfaces.SelectMany(
                surfaceKv => surfaceKv.Value.Resources.Select(
                    resourceKv =>
                    {
                        MineableResource resource = gameData.MineableResources[resourceKv.Key];
                        return new Measurement<long>(
                            resourceKv.Value,
                            baseTags.Concat(
                                new Dictionary<string, object?>
                                {
                                    { "factorio.surface", surfaceKv.Key },
                                    { "factorio.entity", resourceKv.Key },
                                    { "factorio.resource.mineable", true },
                                    { "factorio.resource.category", resource.Category }
                                }
                            )
                        );
                    }
                )
            ),
            "{resource}",
            "The number of mineable resources that has been discovered on the map"
        );

    static void SetupItemInstruments(Meter meter, FactorioGameData gameData, IDictionary<string, object?> baseTags)
    {
        meter.CreateObservableCounter(
            "factorio.game.item.input",
            () => gameData.Forces.SelectMany(
                forceKv => forceKv.Value.Production.Item.Inputs.Select(
                    itemKv => new Measurement<long>(
                        (long)itemKv.Value,
                        baseTags.Concat(
                            new Dictionary<string, object?>
                            {
                                { "factorio.force", forceKv.Key },
                                { "factorio.item", itemKv.Key }
                            }
                        )
                    )
                )
            ),
            "{item}",
            "The number of items produced"
        );

        meter.CreateObservableCounter(
            "factorio.game.item.output",
            () => gameData.Forces.SelectMany(
                forceKv => forceKv.Value.Production.Item.Outputs.Select(
                    itemKv => new Measurement<long>(
                        (long)itemKv.Value,
                        baseTags.Concat(
                            new Dictionary<string, object?>
                            {
                                { "factorio.force", forceKv.Key },
                                { "factorio.item", itemKv.Key }
                            }
                        )
                    )
                )
            ),
            "{item}",
            "The number of items consumed"
        );
    }

    static void SetupFluidInstruments(Meter meter, FactorioGameData gameData, IDictionary<string, object?> baseTags)
    {
        meter.CreateObservableCounter(
            "factorio.game.fluid.input",
            () => gameData.Forces.SelectMany(
                forceKv => forceKv.Value.Production.Fluid.Inputs.Select(
                    itemKv => new Measurement<double>(
                        itemKv.Value,
                        baseTags.Concat(
                            new Dictionary<string, object?>
                            {
                                { "factorio.force", forceKv.Key },
                                { "factorio.fluid", itemKv.Key }
                            }
                        )
                    )
                )
            ),
            "{fluid}",
            "The amount of fluids produced"
        );

        meter.CreateObservableCounter(
            "factorio.game.fluid.output",
            () => gameData.Forces.SelectMany(
                forceKv => forceKv.Value.Production.Fluid.Outputs.Select(
                    itemKv => new Measurement<double>(
                        itemKv.Value,
                        baseTags.Concat(
                            new Dictionary<string, object?>
                            {
                                { "factorio.force", forceKv.Key },
                                { "factorio.fluid", itemKv.Key }
                            }
                        )
                    )
                )
            ),
            "{fluid}",
            "The amount of fluids consumed"
        );
    }

    static void SetupElectricNetworkInstruments(Meter meter, FactorioGameData gameData, IDictionary<string, object?> baseTags)
    {
        meter.CreateObservableUpDownCounter(
            "factorio.game.energy.input",
            () => gameData.Surfaces.SelectMany(
                surfaceKv => surfaceKv.Value.ElectricNetworks.SelectMany(
                    networkKv => networkKv.Value.Flow.Inputs.Select(
                        entityKv =>
                        {
                            ElectricEntity entity = gameData.ElectricEntities[entityKv.Key];
                            return new Measurement<double>(
                                entityKv.Value,
                                baseTags.Concat(
                                    new Dictionary<string, object?>
                                    {
                                        { "factorio.surface", surfaceKv.Key },
                                        { "factorio.network", networkKv.Key },
                                        { "factorio.entity", entityKv.Key },
                                        { "factorio.energy.max_usage", entity.MaxEnergyUsage },
                                        { "factorio.energy.max_production", entity.MaxEnergyProduction },
                                        { "factorio.energy.buffer_capacity", entity.BufferCapacity }
                                    }
                                )
                            );
                        }
                    )
                )
            ),
            "W",
            "The current power being produced"
        );

        meter.CreateObservableUpDownCounter(
            "factorio.game.energy.output",
            () => gameData.Surfaces.SelectMany(
                surfaceKv => surfaceKv.Value.ElectricNetworks.SelectMany(
                    networkKv => networkKv.Value.Flow.Outputs.Select(
                        entityKv =>
                        {
                            ElectricEntity entity = gameData.ElectricEntities[entityKv.Key];
                            return new Measurement<double>(
                                entityKv.Value,
                                baseTags.Concat(
                                    new Dictionary<string, object?>
                                    {
                                        { "factorio.surface", surfaceKv.Key },
                                        { "factorio.network", networkKv.Key },
                                        { "factorio.entity", entityKv.Key },
                                        { "factorio.energy.max_usage", entity.MaxEnergyUsage },
                                        { "factorio.energy.max_production", entity.MaxEnergyProduction },
                                        { "factorio.energy.buffer_capacity", entity.BufferCapacity }
                                    }
                                )
                            );
                        }
                    )
                )
            ),
            "W",
            "The current power being produced"
        );

        meter.CreateObservableUpDownCounter(
            "factorio.game.energy.buffer",
            () => gameData.Surfaces.SelectMany(
                surfaceKv => surfaceKv.Value.ElectricNetworks.SelectMany(
                    networkKv => networkKv.Value.Buffer.Select(
                        entityKv =>
                        {
                            ElectricEntity entity = gameData.ElectricEntities[entityKv.Key];
                            return new Measurement<double>(
                                entityKv.Value,
                                baseTags.Concat(
                                    new Dictionary<string, object?>
                                    {
                                        { "factorio.surface", surfaceKv.Key },
                                        { "factorio.network", networkKv.Key },
                                        { "factorio.entity", entityKv.Key },
                                        { "factorio.energy.max_usage", entity.MaxEnergyUsage },
                                        { "factorio.energy.max_production", entity.MaxEnergyProduction },
                                        { "factorio.energy.buffer_capacity", entity.BufferCapacity }
                                    }
                                )
                            );
                        }
                    )
                )
            ),
            "W",
            "The current power being produced"
        );
    }
}
