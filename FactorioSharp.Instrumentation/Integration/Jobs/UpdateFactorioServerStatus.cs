using FactorioSharp.Instrumentation.Meters;
using FactorioSharp.Instrumentation.Model;
using FactorioSharp.Instrumentation.Scheduling;
using FactorioSharp.Rcon;
using Microsoft.Extensions.Logging;

namespace FactorioSharp.Instrumentation.Integration.Jobs;

class UpdateFactorioServerStatus : Job
{
    readonly ILogger<UpdateFactorioServerStatus> _logger;

    public UpdateFactorioServerStatus(ILogger<UpdateFactorioServerStatus> logger)
    {
        _logger = logger;
    }

    public override Task OnConnectAsync(FactorioRconClient _, FactorioData data, FactorioMeterOptionsInternal __, CancellationToken ___)
    {
        data.Server.IsConnected = true;

        _logger.LogInformation("Server UP");

        return Task.CompletedTask;
    }

    public override Task OnDisconnectAsync(FactorioData data, FactorioMeterOptionsInternal _, CancellationToken __)
    {
        data.Server.IsConnected = false;

        _logger.LogInformation("Server DOWN");

        return Task.CompletedTask;
    }
}
