using OpenTelemetry.Metrics;

namespace FactorioSharp.Instrumentation.Meters;

/// <summary>
///     Add factorio instrumentation to a meter provider
/// </summary>
public static class FactorioInstrumentationOpenTelemetryMeterProviderBuilderExtensions
{
    /// <summary>
    ///     Add factorio instrumentation to a meter provider
    /// </summary>
    public static MeterProviderBuilder AddFactorioInstrumentation(this MeterProviderBuilder builder, Action<FactorioInstrumentationOptions>? configureOptions = null)
    {
        builder.ConfigureServices(services => services.AddFactorioInstrumentation(configureOptions));

        builder.AddMeter(FactorioInstrumentationConstants.MeterName);

        return builder;
    }
}
