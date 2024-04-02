using FactorioSharp.Instrumentation.Meters;
using FactorioSharp.Instrumentation.Model;
using FactorioSharp.Instrumentation.Scheduling;
using FactorioSharp.Rcon;
using Microsoft.Extensions.Logging;

namespace FactorioSharp.Instrumentation.Integration.Jobs.Server;

class UpdateServerModsJob : Job
{
    readonly ILogger<UpdateServerModsJob> _logger;

    public UpdateServerModsJob(ILogger<UpdateServerModsJob> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectAsync(FactorioRconClient client, FactorioData data, FactorioMeterOptionsInternal _, CancellationToken __)
    {
        Dictionary<string, string> mods = await client.ReadAsync(g => g.Game.ActiveMods) ?? new Dictionary<string, string>();
        string activeModsString = string.Join(", ", mods.Select(e => $"{e.Key} v{e.Value}"));

        if (!mods.TryGetValue("base", out string? baseVersion))
        {
            _logger.LogWarning("Could not determine base version, active mods are: {mods}", activeModsString);
        }
        else
        {
            _logger.LogInformation("Factorio version: {version}", baseVersion);
            _logger.LogInformation("Active mods: {mods}", activeModsString);
        }

        data.Server.FactorioVersion = baseVersion;
        data.Server.Mods = mods;
    }
}
