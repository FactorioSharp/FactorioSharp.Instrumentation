using FactoriOTel.Configuration.Exporters;

namespace FactoriOTel.Configuration;

/// <summary>
///     FactoriOTel configuration
/// </summary>
public class FactoriOTelConfiguration
{
    /// <summary>
    ///     The Factorio servers to read data from
    /// </summary>
    public IReadOnlyList<FactorioServerConfiguration> Servers { get; set; } = [];

    /// <summary>
    ///     The exporters to use
    /// </summary>
    public IReadOnlyList<FactorioExporterConfiguration> Exporters { get; set; } = [];
}
