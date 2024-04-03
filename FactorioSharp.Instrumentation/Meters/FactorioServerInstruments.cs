using System.Diagnostics.Metrics;
using FactorioSharp.Instrumentation.Model;

namespace FactorioSharp.Instrumentation.Meters;

class FactorioServerInstruments
{
    public static void Setup(Meter meter, FactorioServerData data, FactorioMeterOptionsInternal _)
    {
        Dictionary<string, object?> tags = new();
        data.EnrichTags(tags);

        meter.CreateObservableUpDownCounter("factorio.server.status", () => data.IsConnected ? 1 : 0, null, "Is the factorio server up ?", tags);
        meter.CreateObservableUpDownCounter("factorio.server.player.count", () => data.Players.Length, null, "The number of players on the factorio server", tags);
        meter.CreateObservableUpDownCounter(
            "factorio.server.connected_player.count",
            () => data.ConnectedPlayers.Length,
            null,
            "The number of players currently connected to the factorio server",
            tags
        );
    }
}
