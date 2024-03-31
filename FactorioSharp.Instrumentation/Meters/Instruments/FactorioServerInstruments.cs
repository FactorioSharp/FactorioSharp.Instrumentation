using System.Diagnostics.Metrics;

namespace FactorioSharp.Instrumentation.Meters.Instruments;

public class FactorioServerInstruments
{
    const string Namespace = "factorio.server";

    public FactorioServerInstruments(Meter meter)
    {
        Status = meter.CreateUpDownCounter<int>($"{Namespace}.status", null, "The current status of the factorio server");
    }

    UpDownCounter<int> Status { get; }
}
