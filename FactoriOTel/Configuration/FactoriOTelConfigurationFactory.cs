using FactoriOTel.Configuration.Exporters;
using FactoriOTel.Configuration.Yaml;

namespace FactoriOTel.Configuration;

static class FactoriOTelConfigurationParser
{
    public static FactoriOTelConfiguration FromYaml(string file)
    {
        using FileStream stream = File.OpenRead(file);
        FactoriOTelYamlConfiguration? yamlConfig = FactoriOTelYamlConfigurationParser.Read(stream);

        if (yamlConfig == null)
        {
            return new FactoriOTelConfiguration();
        }

        IEnumerable<FactorioServerConfiguration> servers = yamlConfig.Servers?.Select(
                                                               serverConfig => new FactorioServerConfiguration
                                                               {
                                                                   Name = serverConfig.Name,
                                                                   RconEndpoint = serverConfig.RconEndpoint,
                                                                   RconPassword = serverConfig.RconPassword
                                                               }
                                                           )
                                                           ?? [];

        IEnumerable<FactorioExporterConfiguration> exporters = new FactorioExporterConfiguration?[]
            {
                yamlConfig.Exporters?.Prometheus,
                yamlConfig.Exporters?.Stdout
            }.Where(c => c != null)
            .Cast<FactorioExporterConfiguration>();

        return new FactoriOTelConfiguration
        {
            Servers = servers.ToArray(),
            Exporters = exporters.ToArray()
        };
    }
}
