using System.Diagnostics.Metrics;
using FactorioSharp.Instrumentation.Model;

namespace FactorioSharp.Instrumentation.Meters;

static class FactorioGameInstruments
{
    public static void Setup(Meter meter, FactorioServerData serverData, FactorioGameData gameData, FactorioMeterOptionsInternal options)
    {
        Dictionary<string, object?> tags = new();
        serverData.EnrichTags(tags);

        foreach (string force in options.MeasuredForces)
        {
            SetupForceInstruments(meter, gameData, force, tags, options);
        }
    }

    static void SetupForceInstruments(Meter meter, FactorioGameData gameData, string force, Dictionary<string, object?> tags, FactorioMeterOptionsInternal options)
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

    static void SetupItemInstruments(Meter meter, FactorioGameData gameData, string force, string item, Dictionary<string, object?> baseTags)
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

    static void SetupFluidInstruments(Meter meter, FactorioGameData gameData, string force, string fluid, Dictionary<string, object?> baseTags)
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
