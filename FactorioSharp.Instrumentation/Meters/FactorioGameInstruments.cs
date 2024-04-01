using System.Diagnostics.Metrics;
using FactorioSharp.Instrumentation.Model;

namespace FactorioSharp.Instrumentation.Meters;

static class FactorioGameInstruments
{
    public static void Setup(Meter meter, FactorioGameData data, FactorioMeterOptionsInternal options)
    {
        foreach (string force in options.MeasuredForces)
        {
            SetupForceInstruments(meter, data, force, options);
        }
    }

    static void SetupForceInstruments(Meter meter, FactorioGameData data, string force, FactorioMeterOptionsInternal options)
    {
        foreach (string item in options.MeasuredItems)
        {
            SetupItemInstruments(meter, data, force, item);
        }

        foreach (string fluid in options.MeasuredFluids)
        {
            SetupFluidInstruments(meter, data, force, fluid);
        }
    }

    static void SetupItemInstruments(Meter meter, FactorioGameData data, string force, string item)
    {
        meter.CreateObservableCounter(
            $"factorio.game.{force}.production.{item}.input",
            () => (long)(data.Forces.GetValueOrDefault(force)?.Production.Item.Inputs.GetValueOrDefault(item) ?? default),
            "{item}",
            $"The number of {item} that has been produced by force {force}",
            new Dictionary<string, object?> { { "factorio.prototype.kind", "item" } }
        );

        meter.CreateObservableCounter(
            $"factorio.game.force.{force}.production.item.{item}.output",
            () => (long)(data.Forces.GetValueOrDefault(force)?.Production.Item.Outputs.GetValueOrDefault(item) ?? default),
            "{item}",
            $"The number of {item} that has been consumed by force {force}",
            new Dictionary<string, object?> { { "factorio.prototype.kind", "item" } }
        );
    }

    static void SetupFluidInstruments(Meter meter, FactorioGameData data, string force, string fluid)
    {
        meter.CreateObservableCounter(
            $"factorio.game.{force}.production.{fluid}.input",
            () => data.Forces.GetValueOrDefault(force)?.Production.Fluid.Inputs.GetValueOrDefault(fluid) ?? default,
            "{volume}",
            $"The quantity of {fluid} that has been produced by force {force}",
            new Dictionary<string, object?> { { "factorio.prototype.kind", "fluid" } }
        );

        meter.CreateObservableCounter(
            $"factorio.game.{force}.production.{fluid}.output",
            () => data.Forces.GetValueOrDefault(force)?.Production.Fluid.Outputs.GetValueOrDefault(fluid) ?? default,
            "{volume}",
            $"The quantity of {fluid} that has been consumed by force {force}",
            new Dictionary<string, object?> { { "factorio.prototype.kind", "fluid" } }
        );
    }
}
