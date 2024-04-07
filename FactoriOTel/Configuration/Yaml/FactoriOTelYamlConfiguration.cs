namespace FactoriOTel.Configuration.Yaml;

class FactoriOTelYamlConfiguration
{
    public FactorioServerConfiguration[]? Servers { get; set; }
    public FactorioExporterYamlConfiguration? Exporters { get; set; }
}
