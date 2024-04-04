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

        foreach (string? surface in options.MeasuredSurfaces)
        {
            SetupSurfaceInstruments(meter, data.Game, surface, tags, options);
        }

        foreach (string force in options.MeasuredForces)
        {
            SetupForceInstruments(meter, data.Game, force, tags, options);
        }
    }

    static void SetupSurfaceInstruments(Meter meter, FactorioGameData gameData, string surface, IDictionary<string, object?> baseTags, FactorioMeterOptionsInternal options)
    {
        foreach (MineableResource resource in gameData.MineableResources)
        {
            SetupMineableResourceInstruments(meter, gameData, surface, resource, baseTags);
        }

        SetupElectricNetworkInstruments(meter, gameData, surface, baseTags);
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

    static void SetupMineableResourceInstruments(Meter meter, FactorioGameData gameData, string surface, MineableResource resource, IDictionary<string, object?> baseTags)
    {
        Dictionary<string, object?> tags = new(baseTags)
            { { "factorio.prototype.kind", "entity" }, { "factorio.resource.category", resource.Category }, { "factorio.resource.mineable", true } };

        meter.CreateObservableUpDownCounter(
            $"factorio.game.{surface}.resource.{resource.Name}",
            () => (long)(gameData.Surfaces.GetValueOrDefault(surface)?.Resources.GetValueOrDefault(resource.Name) ?? default),
            "{resource}",
            $"The number of {resource} that has been discovered on the map",
            tags
        );
    }

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

    static void SetupElectricNetworkInstruments(Meter meter, FactorioGameData gameData, string surface, IDictionary<string, object?> baseTags)
    {
        foreach (ElectricEntity electricEntity in gameData.ElectricEntities)
        {
            Dictionary<string, object?> tags = new(baseTags);
            ElectricEntityType type = electricEntity.Type;

            tags["energy.max_usage"] = electricEntity.MaxEnergyUsage;
            tags["energy.max_production"] = electricEntity.MaxEnergyProduction;
            tags["energy.buffer_capacity"] = electricEntity.BufferCapacity;

            if (type.HasFlag(ElectricEntityType.Sink))
            {
                tags["energy.is_sink"] = true;
            }

            if (type.HasFlag(ElectricEntityType.Source))
            {
                tags["energy.is_source"] = true;
            }

            if (type.HasFlag(ElectricEntityType.Accumulator))
            {
                tags["energy.is_accumulator"] = true;
            }

            meter.CreateObservableUpDownCounter(
                $"factorio.game.{surface}.{electricEntity.Name}.input",
                () => gameData.Surfaces.GetValueOrDefault(surface)?.ElectricNetworks.Sum(kv => kv.Value.Flow.Inputs.GetValueOrDefault(electricEntity.Name)) ?? default,
                "W",
                $"The current power produced by all {electricEntity.Name} on surface {surface}",
                tags
            );

            meter.CreateObservableUpDownCounter(
                $"factorio.game.{surface}.{electricEntity.Name}.output",
                () => gameData.Surfaces.GetValueOrDefault(surface)?.ElectricNetworks.Sum(kv => kv.Value.Flow.Outputs.GetValueOrDefault(electricEntity.Name)) ?? default,
                "W",
                $"The current power produced by all {electricEntity.Name} on surface {surface}",
                tags
            );

            meter.CreateObservableUpDownCounter(
                $"factorio.game.{surface}.{electricEntity.Name}.buffer",
                () => gameData.Surfaces.GetValueOrDefault(surface)?.ElectricNetworks.Sum(kv => kv.Value.Buffer.GetValueOrDefault(electricEntity.Name)) ?? default,
                "J",
                $"The current power stored in all {electricEntity.Name} on surface {surface}",
                tags
            );
        }
    }
}
