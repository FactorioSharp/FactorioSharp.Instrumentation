using System.Diagnostics.Metrics;
using System.Reflection;
using FactorioSharp.Instrumentation.Integration;
using FactorioSharp.Instrumentation.Meters.Instruments;
using Microsoft.Extensions.Logging;

namespace FactorioSharp.Instrumentation.Meters;

public class FactorioMetrics : IDisposable
{
    internal readonly Meter MeterInstance;

    readonly FactorioMetersOptions _options;
    readonly ILoggerFactory? _loggerFactory;
    readonly FactorioClient _factorioClient;

    public FactorioMetrics(string host, int port, string password, FactorioMetersOptions options, ILoggerFactory? loggerFactory = null)
    {
        _options = options;
        _loggerFactory = loggerFactory;
        _factorioClient = new FactorioClient(host, port, password);

        AssemblyName assemblyName = typeof(FactorioMetrics).Assembly.GetName();
        MeterInstance = new Meter(assemblyName.Name!, assemblyName.Version?.ToString());
    }

    public void Initialize()
    {
        new FactorioInstrumentBuilder<int>(_factorioClient, InstrumentType.Gauge, "factorio.server.status", g => 1, description: "Is the server running: 1 (true) or 0 (false)")
        {
            Log = _loggerFactory?.CreateLogger("factorio.server.status"),
            OnException = BehaviorOnException.ReturnDefault
        }.Build(MeterInstance);

        new FactorioInstrumentBuilder<int>(
            _factorioClient,
            InstrumentType.UpDownCounter,
            "factorio.server.player.count",
            g => (int)g.Game.Players.Length,
            "{player}",
            "Total number of players on the map"
        )
        {
            Log = _loggerFactory?.CreateLogger("factorio.server.player.count")
        }.Build(MeterInstance);
    }

    public void Dispose()
    {
        _factorioClient.Disconnect();
        GC.SuppressFinalize(this);
    }
}
