using System.Diagnostics.Metrics;
using FactorioSharp.Instrumentation.Integration.Model;

namespace FactorioSharp.Instrumentation.Meters;

static class FactorioInstruments
{
    public static void Setup(Meter meter, FactorioServerData data, FactorioInstrumentationOptions options)
    {
        SetupServerInstruments(meter, data);

        foreach (string force in options.Forces)
        {
            SetupForceInstruments(meter, data, force, options);
        }
    }

    static void SetupServerInstruments(Meter meter, FactorioServerData data) =>
        meter.CreateObservableGauge("factorio.server.status", () => data.Status ? 1 : 0, "1", "The current status of the factorio server");

    static void SetupForceInstruments(Meter meter, FactorioServerData data, string force, FactorioInstrumentationOptions options)
    {
        foreach (string item in options.Items)
        {
            SetupItemInstruments(meter, data, force, item);
        }

        foreach (string fluid in options.Fluids)
        {
            SetupFluidInstruments(meter, data, force, fluid);
        }
    }

    static void SetupItemInstruments(Meter meter, FactorioServerData data, string force, string item)
    {
        meter.CreateObservableCounter(
            $"factorio.force.{force}.production.item.{item}.input",
            () => data.Forces.GetValueOrDefault(force)?.Production.Item.Inputs.GetValueOrDefault(item) ?? default,
            "{item}",
            $"The number of {item} that has been produced by force {force}"
        );

        meter.CreateObservableCounter(
            $"factorio.force.{force}.production.item.{item}.output",
            () => data.Forces.GetValueOrDefault(force)?.Production.Item.Outputs.GetValueOrDefault(item) ?? default,
            "{item}",
            $"The number of {item} that has been consumed by force {force}"
        );
    }

    static void SetupFluidInstruments(Meter meter, FactorioServerData data, string force, string fluid)
    {
        meter.CreateObservableCounter(
            $"factorio.force.{force}.production.fluid.{fluid}.input",
            () => data.Forces.GetValueOrDefault(force)?.Production.Fluid.Inputs.GetValueOrDefault(fluid) ?? default,
            "{vol}",
            $"The quantity of {fluid} that has been produced by force {force}"
        );

        meter.CreateObservableCounter(
            $"factorio.force.{force}.production.fluid.{fluid}.output",
            () => data.Forces.GetValueOrDefault(force)?.Production.Fluid.Outputs.GetValueOrDefault(fluid) ?? default,
            "{vol}",
            $"The quantity of {fluid} that has been consumed by force {force}"
        );
    }
}
