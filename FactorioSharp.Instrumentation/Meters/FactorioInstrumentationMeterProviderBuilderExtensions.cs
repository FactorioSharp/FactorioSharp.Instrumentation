using FactorioSharp.Instrumentation.Scheduling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MeterProviderBuilder = OpenTelemetry.Metrics.MeterProviderBuilder;
using OpenTelemetryDependencyInjectionMeterProviderBuilderExtensions = OpenTelemetry.Metrics.OpenTelemetryDependencyInjectionMeterProviderBuilderExtensions;

namespace FactorioSharp.Instrumentation.Meters;

public static class FactorioInstrumentationMeterProviderBuilderExtensions
{
    public static MeterProviderBuilder AddFactorioInstrumentation(
        this MeterProviderBuilder builder,
        string host,
        string password,
        Action<FactorioMeterOptions>? configureExporterOptions = null,
        ILoggerFactory? loggerFactory = null
    ) =>
        AddFactorioInstrumentation(builder, host, 27015, password, configureExporterOptions, loggerFactory);

    public static MeterProviderBuilder AddFactorioInstrumentation(
        this MeterProviderBuilder builder,
        string host,
        int port,
        string password,
        Action<FactorioMeterOptions>? configureOptions = null,
        ILoggerFactory? loggerFactory = null
    )
    {
        if (configureOptions != null)
        {
            OpenTelemetryDependencyInjectionMeterProviderBuilderExtensions.ConfigureServices(builder, services => services.Configure(configureOptions));
        }

        OpenTelemetryDependencyInjectionMeterProviderBuilderExtensions.ConfigureServices(
            builder,
            services =>
            {
                services.AddHostedService<FactorioInstrumentationBackgroundWorker>(
                    s => new FactorioInstrumentationBackgroundWorker(
                        host,
                        port,
                        password,
                        s.GetRequiredService<IOptions<FactorioMeterOptions>>(),
                        s.GetRequiredService<ILoggerFactory>()
                    )
                );

                builder.AddMeter(FactorioInstrumentationBackgroundWorker.MeterName);
            }
        );


        return builder;
    }
}
