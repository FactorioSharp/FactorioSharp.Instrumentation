using FactorioSharp.Instrumentation.Meters;
using FactoriOTel.Configuration.Exporters;
using OpenTelemetry.Metrics;

namespace FactoriOTel.Configuration;

static class MeterProviderBuilderConfigurationExtensions
{
    public static MeterProviderBuilder ConfigureServer(this MeterProviderBuilder meterProviderBuilder, FactorioServerConfiguration configuration) =>
        meterProviderBuilder.AddFactorioInstrumentation(
            options =>
            {
                options.Server.Name = configuration.Name;
                options.Server.Uri = new Uri(configuration.RconEndpoint);
                options.Server.RconPassword = configuration.RconPassword;
            }
        );

    public static MeterProviderBuilder ConfigureExporter(this MeterProviderBuilder meterProviderBuilder, FactorioExporterConfiguration configuration) =>
        configuration switch
        {
            FactorioPrometheusExporterConfiguration prometheusExporter => ConfigureExporter(meterProviderBuilder, prometheusExporter),
            FactorioStdoutExporterConfiguration stdoutExporter => ConfigureExporter(meterProviderBuilder, stdoutExporter),
            _ => throw new NotSupportedException($"Exporter {configuration} not supported yet.")
        };

    static MeterProviderBuilder ConfigureExporter(this MeterProviderBuilder meterProviderBuilder, FactorioPrometheusExporterConfiguration configuration) =>
        meterProviderBuilder.AddPrometheusHttpListener(
            options =>
            {
                options.UriPrefixes = [configuration.Endpoint];
                options.ScrapeEndpointPath = configuration.ScrapeEndpointPath;
            }
        );

    static MeterProviderBuilder ConfigureExporter(this MeterProviderBuilder meterProviderBuilder, FactorioStdoutExporterConfiguration _) =>
        meterProviderBuilder.AddConsoleExporter();
}
