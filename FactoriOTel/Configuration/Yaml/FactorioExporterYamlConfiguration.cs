using FactoriOTel.Configuration.Exporters;

namespace FactoriOTel.Configuration.Yaml;

class FactorioExporterYamlConfiguration
{
    public FactorioPrometheusExporterConfiguration? Prometheus { get; set; }
    public FactorioStdoutExporterConfiguration? Stdout { get; set; }
}
