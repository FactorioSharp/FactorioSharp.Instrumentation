using FactorioSharp.Instrumentation.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
        ArgumentNullException.ThrowIfNull(builder);

        FactorioMetersOptions options = new();
        configureExporterOptions?.Invoke(options);

        FactorioMetrics instrumentation = new(ipAddress, port, password, options, loggerFactory?.CreateLogger<FactorioMetrics>() ?? NullLogger<FactorioMetrics>.Instance);
        instrumentation.Initialize().RunSync();

        builder.AddMeter(instrumentation.MeterInstance.Name);

        builder.AddInstrumentation(() => instrumentation);

        return builder;
    }
}
