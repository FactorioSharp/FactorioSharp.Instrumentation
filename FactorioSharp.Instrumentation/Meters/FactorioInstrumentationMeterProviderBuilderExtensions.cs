using FactorioSharp.Instrumentation.Integration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OpenTelemetry.Metrics;

namespace FactorioSharp.Instrumentation.Meters;

public static class FactorioInstrumentationMeterProviderBuilderExtensions
{
    public static MeterProviderBuilder AddFactorioInstrumentation(
        this MeterProviderBuilder builder,
        string host,
        string password,
        Action<FactorioInstrumentationOptions>? configureExporterOptions = null,
        ILoggerFactory? loggerFactory = null
    ) =>
        AddFactorioInstrumentation(builder, host, 27015, password, configureExporterOptions, loggerFactory);

    public static MeterProviderBuilder AddFactorioInstrumentation(
        this MeterProviderBuilder builder,
        string host,
        int port,
        string password,
        Action<FactorioInstrumentationOptions>? configureOptions = null,
        ILoggerFactory? loggerFactory = null
    )
    {
        FactorioInstrumentationOptions options = new();
        configureOptions?.Invoke(options);

        FactorioInstrumentationBackgroundWorker worker = new(host, port, password, options, loggerFactory ?? NullLoggerFactory.Instance);
        builder.ConfigureServices(services => services.AddHostedService<FactorioInstrumentationBackgroundWorker>(_ => worker));

        builder.AddMeter(worker.Meter.Name);
        builder.AddInstrumentation(worker.Meter);


        return builder;
    }
}
