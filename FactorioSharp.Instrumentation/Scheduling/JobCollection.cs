using System.Collections;
using FactorioSharp.Instrumentation.Meters;
using FactorioSharp.Instrumentation.Model;
using FactorioSharp.Rcon;
using Microsoft.Extensions.Logging;

namespace FactorioSharp.Instrumentation.Scheduling;

class JobCollection : IReadOnlyCollection<Job>
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

    public async Task ExecuteOnStartAsync(FactorioData data, FactorioMeasurementOptionsInternal options, CancellationToken stoppingToken)
    {
        foreach (Job job in _jobs)
        {
            await HandleExceptions(() => job.OnStartAsync(data, options, stoppingToken), job, "OnStart");
        }
    }

    public async Task ExecuteOnConnectAsync(FactorioData data, FactorioRconClient client, FactorioMeasurementOptionsInternal options, CancellationToken stoppingToken)
    {
        foreach (Job job in _jobs)
        {
            await HandleExceptions(() => job.OnConnectAsync(client, data, options, stoppingToken), job, "OnConnect");
        }
    }

    public async Task ExecuteOnTickAsync(FactorioData data, FactorioRconClient client, FactorioMeasurementOptionsInternal options, CancellationToken stoppingToken)
    {
        foreach (Job job in _jobs)
        {
            await HandleExceptions(() => job.OnTickAsync(client, data, options, stoppingToken), job, "OnTick");
        }
    }

    public async Task ExecuteOnTickAsync(int index, FactorioData data, FactorioRconClient client, FactorioMeasurementOptionsInternal options, CancellationToken stoppingToken)
    {
        Job? job = _jobs[index];
        await HandleExceptions(() => job.OnTickAsync(client, data, options, stoppingToken), job, "OnTick");
    }

    public async Task ExecuteOnDisconnectAsync(FactorioData data, FactorioMeasurementOptionsInternal options, CancellationToken stoppingToken)
    {
        foreach (Job job in _jobs)
        {
            await HandleExceptions(() => job.OnDisconnectAsync(data, options, stoppingToken), job, "OnDisconnect");
        }
    }

    public async Task ExecuteOnStopAsync(FactorioData data, FactorioMeasurementOptionsInternal options, CancellationToken stoppingToken)
    {
        foreach (Job job in _jobs)
        {
            await HandleExceptions(() => job.OnStopAsync(data, options, stoppingToken), job, "OnStop");
        }
    }

    public int Count => _jobs.Count;
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
