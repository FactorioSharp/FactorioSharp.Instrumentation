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

        UpdateMods(mods, data);
    }

    void UpdateMods(Dictionary<string, string> mods, FactorioData data)
    {
        string[] toDisable = data.Game.Mods.Keys.Except(mods.Keys).ToArray();
        string[] toAdd = mods.Keys.Except(data.Game.Mods.Keys).ToArray();

        foreach (string mod in toDisable)
        {
            data.Game.Mods[mod].IsActive = false;
        }

        foreach (string mod in toAdd)
        {
            data.Game.Mods.AddOrUpdate(
                mod,
                m => new FactorioModData { IsActive = true, Version = mods[m] },
                (m, d) =>
                {
                    d.IsActive = true;
                    d.Version = mods[m];

                    return d;
                }
            );
        }
    }
}
