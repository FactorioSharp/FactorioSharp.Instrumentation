using System.Diagnostics.Metrics;
using FactorioSharp.Instrumentation.Model;

namespace FactorioSharp.Instrumentation.Meters;

class FactorioServerInstruments
{
    public static void Setup(Meter meter, FactorioServerData data, FactorioMeasurementOptionsInternal _)
    {
        Dictionary<string, object?> tags = new();
        data.EnrichTags(tags);

        meter.CreateObservableUpDownCounter("factorio.server.status", () => new Measurement<int>(data.IsUp ? 1 : 0, tags), null, "Is the factorio server up ?");
    }
}
