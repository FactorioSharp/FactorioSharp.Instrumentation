using FactorioSharp.Instrumentation.Scheduling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FactorioSharp.Instrumentation.Meters;

/// <summary>
///     Add factorio instrumentation to a service collection
/// </summary>
public static class FactorioInstrumentationMeterProviderBuilderExtensions
{
    /// <summary>
    ///     Add factorio instrumentation to a service collection
    /// </summary>
    public static IServiceCollection AddFactorioInstrumentation(this IServiceCollection services, Action<FactorioInstrumentationOptions>? configureOptions = null)
    {
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }

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

        return services;
    }
}
