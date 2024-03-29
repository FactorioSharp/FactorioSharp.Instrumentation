using System.Diagnostics.Metrics;
using System.Reflection;
using FactorioRconSharp;
using FactorioSharp.Instrumentation.Extensions;
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
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

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
            "factorio.server.player.count",
            async () => (int)await _factorioClient.ReadAsync(g => g.Game.Players.Length),
            "{player}",
            "Number of connected players"
        );
    }

    public void Dispose()
    {
        _factorioClient.Disconnect();
        GC.SuppressFinalize(this);
    }

    void CreateObservableUpDownCounter<T>(string name, Func<Task<T>> observeValue, string? unit = null, string? description = null) where T: struct =>
        MeterInstance.CreateObservableUpDownCounter(
            name,
            () =>
            {
                try
                {
                    Task<T> resultTask = observeValue();
                    T result = resultTask.RunSync();
                    return result;
                }
                catch (Exception exn)
                {
                    _logger.LogError("An error occured while observing metric {name}: {exn}.\n{exnDetailed}", name, exn.Message, exn);
                    throw;
                }
            },
            unit,
            description
        );
}
