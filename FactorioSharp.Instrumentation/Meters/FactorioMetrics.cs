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
    readonly ILogger<FactorioMetrics> _logger;
    readonly FactorioRconClient _factorioClient;

    public FactorioMetrics(string url, int port, string password, FactorioMetersOptions options, ILogger<FactorioMetrics> logger)
    {
        _password = password;
        _options = options;
        _logger = logger;
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

        CreateObservableUpDownCounter(
            new FactorioInstrument<int>(_factorioClient, "factorio.server.player.count", g => (int)g.Game.Players.Length, "{player}", "Number of connected players")
        );
    }

    public void Dispose()
    {
        _factorioClient.Disconnect();
        GC.SuppressFinalize(this);
    }

    void CreateObservableUpDownCounter<T>(InstrumentBase<T> instrument) where T: struct =>
        MeterInstance.CreateObservableUpDownCounter(
            instrument.Name,
            () =>
            {
                try
                {
                    return instrument.Observe();
                }
                catch (Exception exn)
                {
                    _logger.LogError("An error occured while observing instrument {name}: {exn}.\n{exnDetailed}", instrument.Name, exn.Message, exn);
                    throw;
                }
            },
            instrument.Unit,
            instrument.Description
        );
}
