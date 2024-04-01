using FactorioSharp.Instrumentation.Scheduling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;

namespace FactorioSharp.Instrumentation.Meters;

public static class FactorioInstrumentationMeterProviderBuilderExtensions
{
    public static MeterProviderBuilder AddFactorioInstrumentation(
        this MeterProviderBuilder builder,
        string host,
        string password,
        Action<FactorioMeterOptions>? configureExporterOptions = null
    ) =>
        AddFactorioInstrumentation(builder, host, 27015, password, configureExporterOptions);

    public static MeterProviderBuilder AddFactorioInstrumentation(
        this MeterProviderBuilder builder,
        string host,
        int port,
        string password,
        Action<FactorioMeterOptions>? configureOptions = null
    )
    {
        if (configureOptions != null)
        {
            builder.ConfigureServices(services => services.Configure(configureOptions));
        }

        builder.ConfigureServices(
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
