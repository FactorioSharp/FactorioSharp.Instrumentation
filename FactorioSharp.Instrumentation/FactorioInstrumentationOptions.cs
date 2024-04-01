using FactorioSharp.Instrumentation.Meters;

namespace FactorioSharp.Instrumentation;

public class FactorioInstrumentationOptions
{
    public FactorioServerOptions Server { get; set; } = new();
    public FactorioMeterOptions Meter { get; set; } = new();
}

public class FactorioServerOptions
{
    public Uri? Uri { get; set; }
    public string? RconPassword { get; set; }
    public string? Name { get; set; }
}
