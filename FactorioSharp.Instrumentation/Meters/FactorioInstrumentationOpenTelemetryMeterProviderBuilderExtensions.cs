using OpenTelemetry.Metrics;

namespace FactorioSharp.Instrumentation.Meters;

public static class FactorioInstrumentationOpenTelemetryMeterProviderBuilderExtensions
{
    public static MeterProviderBuilder AddFactorioInstrumentation(this MeterProviderBuilder builder, Action<FactorioInstrumentationOptions>? configureOptions = null)
    {
        builder.ConfigureServices(services => services.AddFactorioInstrumentation(configureOptions));

        builder.AddMeter(FactorioInstrumentationConstants.MeterName);

        return builder;
    }
}
