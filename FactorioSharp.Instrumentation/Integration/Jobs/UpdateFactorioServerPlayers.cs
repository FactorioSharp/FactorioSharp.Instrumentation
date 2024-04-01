using FactorioSharp.Instrumentation.Meters;
using FactorioSharp.Instrumentation.Model;
using FactorioSharp.Instrumentation.Scheduling;
using FactorioSharp.Rcon;
using Microsoft.Extensions.Logging;

namespace FactorioSharp.Instrumentation.Integration.Jobs;

class UpdateFactorioServerPlayers : Job
{
    readonly ILogger<UpdateFactorioServerPlayers> _logger;

    public UpdateFactorioServerPlayers(ILogger<UpdateFactorioServerPlayers> logger)
    {
        _logger = logger;
    }

    public override async Task OnTickAsync(FactorioRconClient client, FactorioData data, FactorioMeterOptionsInternal _, CancellationToken __)
    {
        uint totalPlayerCount = await client.ReadAsync(g => g.Game.Players.Length);

        List<string> players = [];
        for (int index = 0; index < totalPlayerCount; index++)
        {
            string player = await client.ReadAsync((g, i) => g.Game.Players[i + 1].Name, (uint)index);
            players.Add(player);
        }

        data.Server.Players = players.ToArray();

        int connectedPlayerCount = await client.ReadAsync(g => g.Game.ConnectedPlayers.Length);

        List<string> connectedPlayers = [];
        for (int index = 0; index < connectedPlayerCount; index++)
        {
            string player = await client.ReadAsync((g, i) => g.Game.ConnectedPlayers[i + 1].Name, index);
            connectedPlayers.Add(player);
        }

        data.Server.ConnectedPlayers = connectedPlayers.ToArray();
    }
}
