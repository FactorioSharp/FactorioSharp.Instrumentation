using FactorioSharp.Instrumentation.Scheduling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;

namespace FactorioSharp.Instrumentation.Meters;

public static class FactorioInstrumentationMeterProviderBuilderExtensions
{
    public static MeterProviderBuilder AddFactorioInstrumentation(this MeterProviderBuilder builder, Action<FactorioInstrumentationOptions>? configureOptions = null)
    {
        if (configureOptions != null)
        {
            builder.ConfigureServices(services => services.Configure(configureOptions));
        }

        builder.ConfigureServices(
            services =>
            {
                services.AddHostedService<FactorioInstrumentationBackgroundWorker>(
                    s =>
                    {
                        IOptions<FactorioInstrumentationOptions> options = s.GetRequiredService<IOptions<FactorioInstrumentationOptions>>();
                        ILoggerFactory loggerFactory = s.GetRequiredService<ILoggerFactory>();

                        if (options.Value.Server.Uri == null)
                        {
                            throw new InvalidOperationException("Factorio server URI must be configured");
                        }

                        if (options.Value.Server.RconPassword == null)
                        {
                            throw new InvalidOperationException("Factorio server RCON password must be configured");
                        }

                        return new FactorioInstrumentationBackgroundWorker(options, loggerFactory);
                    }
                );

                builder.AddMeter(FactorioInstrumentationBackgroundWorker.MeterName);
            }
        );


        return builder;
    }
}
