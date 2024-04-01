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

    public override Task OnConnectAsync(
        FactorioRconClient client,
        FactorioServerData serverData,
        FactorioGameData gameData,
        FactorioMeterOptionsInternal options,
        CancellationToken cancellationToken
    )
    {
        serverData.IsConnected = true;

        _logger.LogInformation("Server UP");

        return Task.CompletedTask;
    }

    public override Task OnDisconnectAsync(FactorioServerData serverData, FactorioGameData gameData, FactorioMeterOptionsInternal options, CancellationToken cancellationToken)
    {
        serverData.IsConnected = false;

        _logger.LogInformation("Server DOWN");

        return Task.CompletedTask;
    }
}
