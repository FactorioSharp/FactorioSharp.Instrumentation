using System.Diagnostics.Metrics;
using FactorioSharp.Instrumentation.Model;

namespace FactorioSharp.Instrumentation.Meters;

class FactorioServerInstruments
{
    public static void Setup(Meter meter, FactorioServerData data, FactorioMeterOptionsInternal _)
    {
        Dictionary<string, object?> tags = new();
        data.EnrichTags(tags);

        meter.CreateObservableUpDownCounter("factorio.server.status", () => data.IsUp ? 1 : 0, null, "Is the factorio server up ?", tags);
    }
}
