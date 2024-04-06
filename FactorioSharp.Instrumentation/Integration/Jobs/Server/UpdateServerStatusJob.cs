using FactorioSharp.Instrumentation.Meters;
using FactorioSharp.Instrumentation.Model;
using FactorioSharp.Instrumentation.Scheduling;
using FactorioSharp.Rcon;
using Microsoft.Extensions.Logging;

namespace FactorioSharp.Instrumentation.Integration.Jobs.Server;

class UpdateServerStatusJob : Job
{
    readonly ILogger<UpdateServerStatusJob> _logger;

    public UpdateServerStatusJob(ILogger<UpdateServerStatusJob> logger)
    {
        _logger = logger;
    }

    public override Task OnConnectAsync(FactorioRconClient _, FactorioData data, FactorioMeasurementOptionsInternal __, CancellationToken ___)
    {
        data.Server.IsUp = true;

        _logger.LogInformation("Server UP");

        return Task.CompletedTask;
    }

    public override Task OnDisconnectAsync(FactorioData data, FactorioMeasurementOptionsInternal _, CancellationToken __)
    {
        data.Server.IsUp = false;

        _logger.LogInformation("Server DOWN");

        return Task.CompletedTask;
    }
}
