using FactorioSharp.Instrumentation.Meters;
using FactorioSharp.Instrumentation.Model;
using FactorioSharp.Instrumentation.Scheduling;
using FactorioSharp.Rcon;
using Microsoft.Extensions.Logging;

namespace FactorioSharp.Instrumentation.Integration.Jobs.Game;

class UpdateGameJob : Job
{
    readonly ILogger<UpdateGameJob> _logger;

    public UpdateGameJob(ILogger<UpdateGameJob> logger)
    {
        _logger = logger;
    }

    public override async Task OnTickAsync(FactorioRconClient client, FactorioData data, FactorioMeasurementOptionsInternal options, CancellationToken cancellationToken)
    {
        bool paused = data.Game.Time.Paused;

        data.Game.Time.Tick = await client.ReadAsync(g => g.Game.Tick);
        data.Game.Time.TicksPlayed = await client.ReadAsync(g => g.Game.TicksPlayed);
        data.Game.Time.Paused = await client.ReadAsync(g => g.Game.TickPaused);
        data.Game.Time.Speed = await client.ReadAsync(g => g.Game.Speed);

        if (paused && !data.Game.Time.Paused)
        {
            _logger.LogInformation("Game resumed");
        }
        else if (!paused && data.Game.Time.Paused)
        {
            _logger.LogInformation("Game paused");
        }
    }
}
