using System.Collections;
using FactorioSharp.Instrumentation.Meters;
using FactorioSharp.Instrumentation.Model;
using FactorioSharp.Rcon;

namespace FactorioSharp.Instrumentation.Scheduling;

class JobCollection : IEnumerable<Job>
{
    readonly List<Job> _jobs = [];

    public void Add(Job item) => _jobs.Add(item);

    public async Task ExecuteOnStartAsync(FactorioGameData data, FactorioMeterOptionsInternal options, CancellationToken stoppingToken)
    {
        foreach (Job job in _jobs)
        {
            await job.OnStartAsync(data, options, stoppingToken);
        }
    }

    public async Task ExecuteOnConnectAsync(FactorioGameData data, FactorioRconClient client, FactorioMeterOptionsInternal options, CancellationToken stoppingToken)
    {
        foreach (Job job in _jobs)
        {
            await job.OnConnectAsync(client, data, options, stoppingToken);
        }
    }

    public async Task ExecuteOnTickAsync(FactorioGameData data, FactorioRconClient client, FactorioMeterOptionsInternal options, CancellationToken stoppingToken)
    {
        foreach (Job job in _jobs)
        {
            await job.OnTickAsync(client, data, options, stoppingToken);
        }
    }

    public async Task ExecuteOnDisconnectAsync(FactorioGameData data, FactorioMeterOptionsInternal options, CancellationToken stoppingToken)
    {
        foreach (Job job in _jobs)
        {
            await job.OnDisconnectAsync(data, options, stoppingToken);
        }
    }

    public async Task ExecuteOnStopAsync(FactorioGameData data, FactorioMeterOptionsInternal options, CancellationToken stoppingToken)
    {
        foreach (Job job in _jobs)
        {
            await job.OnStopAsync(data, options, stoppingToken);
        }
    }

    public IEnumerator<Job> GetEnumerator() => _jobs.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_jobs).GetEnumerator();
}
