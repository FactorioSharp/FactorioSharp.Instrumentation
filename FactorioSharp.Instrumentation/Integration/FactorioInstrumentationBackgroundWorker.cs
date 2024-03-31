using System.Diagnostics.Metrics;
using System.Reflection;
using FactorioSharp.Instrumentation.Meters.Instruments;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FactorioSharp.Instrumentation.Integration;

/// <summary>
///     Collect data from the factorio server periodically and emit it on the available meters
/// </summary>
class FactorioInstrumentationBackgroundWorker : BackgroundService
{
    readonly FactorioInstrumentationOptions _options;
    readonly ILoggerFactory _loggerFactory;
    readonly FactorioClient _factorioClient;

    public Meter Meter { get; }
    public FactorioInstruments Instruments { get; }

    public FactorioInstrumentationBackgroundWorker(string host, int port, string password, FactorioInstrumentationOptions options, ILoggerFactory loggerFactory)
    {
        _options = options;
        _loggerFactory = loggerFactory;
        _factorioClient = new FactorioClient(host, port, password);

        AssemblyName assemblyName = typeof(FactorioInstruments).Assembly.GetName();
        Meter = new Meter(assemblyName.Name!, assemblyName.Version?.ToString());

        Instruments = new FactorioInstruments(Meter);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) => throw new NotImplementedException();
}
