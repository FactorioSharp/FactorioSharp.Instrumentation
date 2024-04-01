using System.Diagnostics.Metrics;
using FactorioSharp.Instrumentation.Model;

namespace FactorioSharp.Instrumentation.Meters;

class FactorioServerInstruments
{
    public static void Setup(Meter meter, FactorioServerData data, FactorioMeterOptionsInternal options)
    {
        Dictionary<string, object?> tags = new();
        data.EnrichTags(tags);

        meter.CreateObservableUpDownCounter("factorio.server.status", () => data.IsConnected ? 1 : 0, null, "The current status of the factorio server", tags);
    }
}
