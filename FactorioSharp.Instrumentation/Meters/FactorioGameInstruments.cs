using System.Diagnostics.Metrics;
using FactorioSharp.Instrumentation.Model;

namespace FactorioSharp.Instrumentation.Meters;

static class FactorioGameInstruments
{
    public static void Setup(Meter meter, FactorioData data, FactorioMeterOptionsInternal options)
    {
        Dictionary<string, object?> tags = new();
        data.Server.EnrichTags(tags);

        SetupGameInstruments(meter, data.Game, tags, options);
        SetupMineableResourceInstruments(meter, data.Game, tags);
        SetupElectricNetworkInstruments(meter, data.Game, tags);

        foreach (string force in options.MeasuredForces)
        {
            SetupForceInstruments(meter, data.Game, force, tags, options);
        }
    }

    static void SetupForceInstruments(Meter meter, FactorioGameData gameData, string force, IDictionary<string, object?> tags, FactorioMeterOptionsInternal options)
    {
        foreach (string item in options.MeasuredItems)
        {
            SetupItemInstruments(meter, gameData, force, item, tags);
        }

        foreach (string fluid in options.MeasuredFluids)
        {
            SetupFluidInstruments(meter, gameData, force, fluid, tags);
        }
    }

    static void SetupGameInstruments(Meter meter, FactorioGameData gameData, Dictionary<string, object?> tags, FactorioMeterOptionsInternal options)
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

    static void SetupItemInstruments(Meter meter, FactorioGameData gameData, string force, string item, IDictionary<string, object?> baseTags)
    {
        Dictionary<string, object?> tags = new(baseTags) { { "factorio.prototype.kind", "item" } };

        meter.CreateObservableCounter(
            $"factorio.game.{force}.production.{item}.input",
            () => (long)(gameData.Forces.GetValueOrDefault(force)?.Production.Item.Inputs.GetValueOrDefault(item) ?? default),
            "{item}",
            $"The number of {item} that has been produced by force {force}",
            tags
        );

        meter.CreateObservableCounter(
            $"factorio.game.{force}.production.{item}.output",
            () => (long)(gameData.Forces.GetValueOrDefault(force)?.Production.Item.Outputs.GetValueOrDefault(item) ?? default),
            "{item}",
            $"The number of {item} that has been consumed by force {force}",
            tags
        );
    }

    static void SetupFluidInstruments(Meter meter, FactorioGameData gameData, string force, string fluid, IDictionary<string, object?> baseTags)
    {
        Dictionary<string, object?> tags = new(baseTags) { { "factorio.prototype.kind", "fluid" } };

        meter.CreateObservableCounter(
            $"factorio.game.{force}.production.{fluid}.input",
            () => gameData.Forces.GetValueOrDefault(force)?.Production.Fluid.Inputs.GetValueOrDefault(fluid) ?? default,
            "{volume}",
            $"The quantity of {fluid} that has been produced by force {force}",
            tags
        );

        meter.CreateObservableCounter(
            $"factorio.game.{force}.production.{fluid}.output",
            () => gameData.Forces.GetValueOrDefault(force)?.Production.Fluid.Outputs.GetValueOrDefault(fluid) ?? default,
            "{volume}",
            $"The quantity of {fluid} that has been consumed by force {force}",
            tags
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
