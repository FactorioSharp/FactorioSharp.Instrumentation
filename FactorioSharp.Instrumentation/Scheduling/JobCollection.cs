using System.Collections;
using FactorioSharp.Instrumentation.Meters;
using FactorioSharp.Instrumentation.Model;
using FactorioSharp.Rcon;
using Microsoft.Extensions.Logging;

namespace FactorioSharp.Instrumentation.Scheduling;

class JobCollection : IEnumerable<Job>
{
    readonly List<Job> _jobs;
    readonly ILogger<JobCollection> _logger;

    public JobCollection(ILogger<JobCollection> logger) : this([], logger)
    {
    }

    public JobCollection(IEnumerable<Job> jobs, ILogger<JobCollection> logger)
    {
        _jobs = new List<Job>(jobs);
        _logger = logger;
    }

    public void Add(Job item) => _jobs.Add(item);

    public async Task ExecuteOnStartAsync(FactorioGameData data, FactorioMeterOptionsInternal options, CancellationToken stoppingToken)
    {
        foreach (Job job in _jobs)
        {
            await HandleExceptions(() => job.OnStartAsync(data, options, stoppingToken), job, "OnStart");
        }
    }

    public async Task ExecuteOnConnectAsync(
        FactorioServerData serverData,
        FactorioGameData gameData,
        FactorioRconClient client,
        FactorioMeterOptionsInternal options,
        CancellationToken stoppingToken
    )
    {
        foreach (Job job in _jobs)
        {
            await HandleExceptions(() => job.OnConnectAsync(client, serverData, gameData, options, stoppingToken), job, "OnConnect");
        }
    }

    public async Task ExecuteOnTickAsync(FactorioGameData data, FactorioRconClient client, FactorioMeterOptionsInternal options, CancellationToken stoppingToken)
    {
        foreach (Job job in _jobs)
        {
            await HandleExceptions(() => job.OnTickAsync(client, data, options, stoppingToken), job, "OnTick");
        }
    }

    public async Task ExecuteOnDisconnectAsync(FactorioServerData serverData, FactorioGameData gameData, FactorioMeterOptionsInternal options, CancellationToken stoppingToken)
    {
        foreach (Job job in _jobs)
        {
            await HandleExceptions(() => job.OnDisconnectAsync(serverData, gameData, options, stoppingToken), job, "OnDisconnect");
        }
    }

    public async Task ExecuteOnStopAsync(FactorioGameData data, FactorioMeterOptionsInternal options, CancellationToken stoppingToken)
    {
        foreach (Job job in _jobs)
        {
            await HandleExceptions(() => job.OnStopAsync(data, options, stoppingToken), job, "OnStop");
        }
    }

    public IEnumerator<Job> GetEnumerator() => _jobs.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_jobs).GetEnumerator();

    async Task HandleExceptions(Func<Task> taskFactory, Job job, string trigger)
    {
        try
        {
            await taskFactory();
        }
        catch (Exception exn)
        {
            _logger.LogError(exn, "An error occured while executing trigger {trigger} of job {job}", trigger, job);
        }
    }
}
