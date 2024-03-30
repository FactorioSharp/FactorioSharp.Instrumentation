using System.Diagnostics.Metrics;
using System.Reflection;
using FactorioSharp.Instrumentation.Meters.Instruments;
using FactorioSharp.Rcon;
using Microsoft.Extensions.Logging;

namespace FactorioSharp.Instrumentation.Meters;

public class FactorioMetrics : IDisposable
{
    internal readonly Meter MeterInstance;

    readonly string _password;
    readonly FactorioMetersOptions _options;
    readonly ILoggerFactory? _loggerFactory;
    readonly FactorioRconClient _factorioClient;

    public FactorioMetrics(string url, int port, string password, FactorioMetersOptions options, ILoggerFactory? loggerFactory = null)
    {
        _password = password;
        _options = options;
        _loggerFactory = loggerFactory;
        _factorioClient = new FactorioRconClient(url, port);

        AssemblyName assemblyName = typeof(FactorioMetrics).Assembly.GetName();
        MeterInstance = new Meter(assemblyName.Name!, assemblyName.Version?.ToString());
    }

    public async Task Initialize()
    {
        if (!await _factorioClient.ConnectAsync(_password))
        {
            throw new InvalidOperationException("Could not connect to factorio server");
        }

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
