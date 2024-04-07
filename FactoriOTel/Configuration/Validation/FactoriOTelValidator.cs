using FactoriOTel.Configuration.Exporters;

namespace FactoriOTel.Configuration.Validation;

static class FactoriOTelValidator
{
    public static FactoriOTelValidationResult Validate(FactoriOTelConfiguration configuration)
    {
        List<string> errors = new();

        ValidateServers(configuration.Servers, errors);
        ValidateExporter(configuration.Exporters, errors);

        return new FactoriOTelValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }

    static void ValidateServers(IReadOnlyList<FactorioServerConfiguration> servers, List<string> errors)
    {
        if (servers.Count == 0)
        {
            errors.Add("No server was configured");
        }

        for (int index = 0; index < servers.Count; index++)
        {
            FactorioServerConfiguration server = servers[index];

            if (string.IsNullOrWhiteSpace(server.Name))
            {
                errors.Add($"Server name not set ({index})");
            }

            if (string.IsNullOrWhiteSpace(server.RconEndpoint))
            {
                errors.Add($"Server RCON endpoint not set ({index})");
            }

            if (string.IsNullOrWhiteSpace(server.RconPassword))
            {
                errors.Add($"Server RCON password not set ({index})");
            }
        }
    }

    static void ValidateExporter(IReadOnlyList<FactorioExporterConfiguration> exporters, List<string> errors)
    {
        if (exporters.Count == 0)
        {
            errors.Add("No exporter was configured");
        }

        for (int index = 0; index < exporters.Count; index++)
        {
            FactorioExporterConfiguration exporter = exporters[index];

            switch (exporter)
            {
                case FactorioPrometheusExporterConfiguration prometheusExporter:
                    ValidateExporter(index, prometheusExporter, errors);
                    break;
            }
        }
    }

    static void ValidateExporter(int index, FactorioPrometheusExporterConfiguration exporter, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(exporter.Endpoint))
        {
            errors.Add($"No endpoint specified for the prometheus exporter ({index})");
        }
    }
}

class FactoriOTelValidationResult
{
    public bool IsValid { get; set; }
    public required IReadOnlyCollection<string> Errors { get; set; }
}
