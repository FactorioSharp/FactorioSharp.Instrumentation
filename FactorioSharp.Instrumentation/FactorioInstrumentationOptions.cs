using FactorioSharp.Instrumentation.Meters;

namespace FactorioSharp.Instrumentation;

/// <summary>
/// Options of the factorio instrumentation library
/// </summary>
public class FactorioInstrumentationOptions
{
    /// <inheritdoc cref="FactorioServerOptions"/>
    public FactorioServerOptions Server { get; set; } = new();
    
    /// <inheritdoc cref="FactorioMeasurementOptions"/>
    public FactorioMeasurementOptions Measurement { get; set; } = new();
}