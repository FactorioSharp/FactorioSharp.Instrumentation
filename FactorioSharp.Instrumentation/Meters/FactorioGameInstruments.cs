﻿using System.Diagnostics.Metrics;
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
        meter.CreateObservableUpDownCounter(
            "factorio.game.player",
            () => new Measurement<long>(gameData.Players.Count, tags),
            "{player}",
            "The number of players on the factorio server"
        );
        meter.CreateObservableUpDownCounter(
            "factorio.game.player.connected",
            () => new Measurement<long>(gameData.Players.Count(kv => kv.Value.IsOnline), tags),
            "{player}",
            "The number of players currently connected to the factorio server"
        );

        meter.CreateObservableCounter("factorio.game.tick", () => new Measurement<long>(gameData.Time.Tick, tags), "{tick}", "The current map tick");
        meter.CreateObservableCounter(
            "factorio.game.tick.played",
            () => new Measurement<long>(gameData.Time.TicksPlayed, tags),
            "{tick}",
            "The number of ticks since the game was created"
        );
        meter.CreateObservableGauge("factorio.game.tick.speed_ratio", () => new Measurement<float>(gameData.Time.Speed, tags), "{tick}", "The speed to update the map at");
        meter.CreateObservableUpDownCounter("factorio.game.tick.paused", () => new Measurement<int>(gameData.Time.Paused ? 1 : 0, tags), "{tick}", "Is the game paused ?");
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
            "factorio.game.electricity.input",
            () => gameData.Surfaces.SelectMany(
                surfaceKv => surfaceKv.Value.ElectricNetworks.SelectMany(
                    networkKv => networkKv.Value.Flow.Inputs.Select(
                        entityKv => new Measurement<double>(
                            entityKv.Value,
                            baseTags.Concat(
                                new Dictionary<string, object?>
                                {
                                    { "factorio.surface", surfaceKv.Key },
                                    { "factorio.network", networkKv.Key },
                                    { "factorio.entity", entityKv.Key },
                                    { "factorio.entity.type", gameData.ElectricEntities[entityKv.Key].Type }
                                }
                            )
                        )
                    )
                )
            ),
            "J/tick",
            "The current electricity being produced"
        );

        meter.CreateObservableUpDownCounter(
            "factorio.game.electricity.output",
            () => gameData.Surfaces.SelectMany(
                surfaceKv => surfaceKv.Value.ElectricNetworks.SelectMany(
                    networkKv => networkKv.Value.Flow.Outputs.Select(
                        entityKv => new Measurement<double>(
                            entityKv.Value,
                            baseTags.Concat(
                                new Dictionary<string, object?>
                                {
                                    { "factorio.surface", surfaceKv.Key },
                                    { "factorio.network", networkKv.Key },
                                    { "factorio.entity", entityKv.Key },
                                    { "factorio.entity.type", gameData.ElectricEntities[entityKv.Key].Type }
                                }
                            )
                        )
                    )
                )
            ),
            "J/tick",
            "The current electricity being produced"
        );

        meter.CreateObservableUpDownCounter(
            "factorio.game.electricity.buffer",
            () => gameData.Surfaces.SelectMany(
                surfaceKv => surfaceKv.Value.ElectricNetworks.SelectMany(
                    networkKv => networkKv.Value.Buffer.Select(
                        entityKv => new Measurement<double>(
                            entityKv.Value,
                            baseTags.Concat(
                                new Dictionary<string, object?>
                                {
                                    { "factorio.surface", surfaceKv.Key },
                                    { "factorio.network", networkKv.Key },
                                    { "factorio.entity", entityKv.Key },
                                    { "factorio.entity.type", gameData.ElectricEntities[entityKv.Key].Type }
                                }
                            )
                        )
                    )
                )
            ),
            "J",
            "The current electricity being produced"
        );

        meter.CreateObservableUpDownCounter(
            "factorio.game.electricity.entity",
            () => gameData.Surfaces.SelectMany(
                surfaceKv => surfaceKv.Value.ElectricNetworks.SelectMany(
                    networkKv => networkKv.Value.Entities.Select(
                        entityKv => new Measurement<double>(
                            entityKv.Value,
                            baseTags.Concat(
                                new Dictionary<string, object?>
                                {
                                    { "factorio.surface", surfaceKv.Key },
                                    { "factorio.network", networkKv.Key },
                                    { "factorio.entity", entityKv.Key },
                                    { "factorio.entity.type", gameData.ElectricEntities[entityKv.Key].Type }
                                }
                            )
                        )
                    )
                )
            ),
            "{entity}",
            "The number of entities producing and consuming electricity"
        );

        meter.CreateObservableUpDownCounter(
            "factorio.game.electricity.input.min",
            () => gameData.Surfaces.SelectMany(
                surfaceKv => surfaceKv.Value.ElectricNetworks.SelectMany(
                    networkKv => networkKv.Value.Entities.Select(
                        entityKv => new Measurement<double>(
                            entityKv.Value * gameData.ElectricEntities[entityKv.Key].MinEnergyUsage,
                            baseTags.Concat(
                                new Dictionary<string, object?>
                                {
                                    { "factorio.surface", surfaceKv.Key },
                                    { "factorio.network", networkKv.Key },
                                    { "factorio.entity", entityKv.Key },
                                    { "factorio.entity.type", gameData.ElectricEntities[entityKv.Key].Type }
                                }
                            )
                        )
                    )
                )
            ),
            "J/tick",
            "The current electricity being produced"
        );

        meter.CreateObservableUpDownCounter(
            "factorio.game.electricity.input.max",
            () => gameData.Surfaces.SelectMany(
                surfaceKv => surfaceKv.Value.ElectricNetworks.SelectMany(
                    networkKv => networkKv.Value.Entities.Select(
                        entityKv => new Measurement<double>(
                            entityKv.Value * gameData.ElectricEntities[entityKv.Key].MaxEnergyUsage,
                            baseTags.Concat(
                                new Dictionary<string, object?>
                                {
                                    { "factorio.surface", surfaceKv.Key },
                                    { "factorio.network", networkKv.Key },
                                    { "factorio.entity", entityKv.Key },
                                    { "factorio.entity.type", gameData.ElectricEntities[entityKv.Key].Type }
                                }
                            )
                        )
                    )
                )
            ),
            "J/tick",
            "The current electricity being produced"
        );

        meter.CreateObservableUpDownCounter(
            "factorio.game.electricity.output.max",
            () => gameData.Surfaces.SelectMany(
                surfaceKv => surfaceKv.Value.ElectricNetworks.SelectMany(
                    networkKv => networkKv.Value.Entities.Select(
                        entityKv => new Measurement<double>(
                            entityKv.Value * gameData.ElectricEntities[entityKv.Key].MaxEnergyProduction,
                            baseTags.Concat(
                                new Dictionary<string, object?>
                                {
                                    { "factorio.surface", surfaceKv.Key },
                                    { "factorio.network", networkKv.Key },
                                    { "factorio.entity", entityKv.Key },
                                    { "factorio.entity.type", gameData.ElectricEntities[entityKv.Key].Type }
                                }
                            )
                        )
                    )
                )
            ),
            "J/tick",
            "The current electricity being produced"
        );

        meter.CreateObservableUpDownCounter(
            "factorio.game.electricity.buffer.max",
            () => gameData.Surfaces.SelectMany(
                surfaceKv => surfaceKv.Value.ElectricNetworks.SelectMany(
                    networkKv => networkKv.Value.Entities.Select(
                        entityKv => new Measurement<double>(
                            entityKv.Value * gameData.ElectricEntities[entityKv.Key].BufferCapacity,
                            baseTags.Concat(
                                new Dictionary<string, object?>
                                {
                                    { "factorio.surface", surfaceKv.Key },
                                    { "factorio.network", networkKv.Key },
                                    { "factorio.entity", entityKv.Key },
                                    { "factorio.entity.type", gameData.ElectricEntities[entityKv.Key].Type }
                                }
                            )
                        )
                    )
                )
            ),
            "J",
            "The current electricity being produced"
        );
    }
}
