namespace FactoriOTel.Configuration.Exporters;

/// <summary>
///     Prometheus exporter configuration
/// </summary>
public class FactorioPrometheusExporterConfiguration : FactorioExporterConfiguration
{
    /// <summary>
    ///     Endpoint on which the prometheus data should be served. <br />
    ///     Defaults to <c>localhost:9464</c>
    /// </summary>
    public required string Endpoint { get; set; } = "localhost:9464";

    /// <summary>
    ///     The path to use for the scraping endpoint. <br />
    ///     Defaults to <c>/metrics</c>
    /// </summary>
    public required string ScrapeEndpointPath { get; set; } = "/metrics";
}
