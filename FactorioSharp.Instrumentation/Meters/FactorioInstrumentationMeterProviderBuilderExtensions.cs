using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;

namespace FactorioSharp.Instrumentation.Meters;

public static class FactorioInstrumentationMeterProviderBuilderExtensions
{
    public static MeterProviderBuilder AddFactorioInstrumentation(
        this MeterProviderBuilder builder,
        string ipAddress,
        string password,
        Action<FactorioMetersOptions>? configureExporterOptions = null,
        ILoggerFactory? loggerFactory = null
    ) =>
        AddFactorioInstrumentation(builder, ipAddress, 27015, password, configureExporterOptions, loggerFactory);

    public static MeterProviderBuilder AddFactorioInstrumentation(
        this MeterProviderBuilder builder,
        string ipAddress,
        int port,
        string password,
        Action<FactorioMetersOptions>? configureExporterOptions = null,
        ILoggerFactory? loggerFactory = null
    )
    {
        FactorioMetersOptions options = new();
        configureExporterOptions?.Invoke(options);

        FactorioMetrics instrumentation = new(ipAddress, port, password, options, loggerFactory);
        instrumentation.Initialize();

        builder.AddMeter(instrumentation.MeterInstance.Name);

        builder.AddInstrumentation(() => instrumentation);

        return builder;
    }
}
