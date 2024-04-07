using System.Text.Json.Serialization;

namespace FactoriOTel.Configuration.Exporters;

/// <summary>
///     Base class for the possible OpenTelemetry exporters
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(FactorioPrometheusExporterConfiguration), "prometheus")]
[JsonDerivedType(typeof(FactorioStdoutExporterConfiguration), "stdout")]
public abstract class FactorioExporterConfiguration
{
}
