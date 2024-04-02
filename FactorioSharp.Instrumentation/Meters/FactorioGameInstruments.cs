using System.Diagnostics.Metrics;
using FactorioSharp.Instrumentation.Model;

namespace FactorioSharp.Instrumentation.Meters;

static class FactorioGameInstruments
{
    public static void Setup(Meter meter, FactorioData data, FactorioMeterOptionsInternal options)
    {
        Dictionary<string, object?> tags = new();
        data.Server.EnrichTags(tags);

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
}
