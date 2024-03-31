using System.Diagnostics.Metrics;

namespace FactorioSharp.Instrumentation.Meters.Instruments;

class FactorioInstruments
{
    FactorioServerInstruments Server { get; }

    public FactorioInstruments(Meter meter)
    {
        Server = new FactorioServerInstruments(meter);
    }
}
