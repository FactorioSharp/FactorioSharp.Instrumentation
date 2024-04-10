using FactorioSharp.Instrumentation.Meters;
using FactorioSharp.Instrumentation.Model;
using FactorioSharp.Instrumentation.Scheduling;
using FactorioSharp.Rcon;
using Microsoft.Extensions.Logging;

namespace FactorioSharp.Instrumentation.Integration.Jobs.Server;

class UpdateServerPlayersJob : Job
{
    readonly ILogger<UpdateServerPlayersJob> _logger;

    public UpdateServerPlayersJob(ILogger<UpdateServerPlayersJob> logger)
    {
        _logger = logger;
    }

    public override async Task OnTickAsync(FactorioRconClient client, FactorioData data, FactorioMeasurementOptionsInternal _, CancellationToken __)
    {
        uint totalPlayerCount = await client.ReadAsync(g => g.Game.Players.Length);

        List<string> players = [];
        for (int index = 0; index < totalPlayerCount; index++)
        {
            string? player = await client.ReadAsync((g, i) => g.Game.Players[i + 1].Name, (uint)index);
            if (player == null)
            {
                continue;
            }

            players.Add(player);
        }

        UpdatePlayers(players, data);

        int connectedPlayerCount = await client.ReadAsync(g => g.Game.ConnectedPlayers.Count);

        HashSet<string> connectedPlayers = [];
        for (int index = 0; index < connectedPlayerCount; index++)
        {
            string? player = await client.ReadAsync((g, i) => g.Game.ConnectedPlayers[i + 1].Name, index);
            if (player == null)
            {
                continue;
            }

            connectedPlayers.Add(player);
        }

        UpdateConnectedPlayers(connectedPlayers, data);
    }

    void UpdatePlayers(IReadOnlyCollection<string> players, FactorioData data)
    {
        string[] toRemove = data.Game.Players.Keys.Except(players).ToArray();

        if (toRemove.Length > 0)
        {
            foreach (string player in toRemove)
            {
                data.Game.Players.Remove(player, out _);
            }

            _logger.LogInformation("Players removed: {players}", string.Join(", ", toRemove));
        }

        string[] toAdd = players.Except(data.Game.Players.Keys).ToArray();

        if (toAdd.Length > 0)
        {
            foreach (string player in toAdd)
            {
                data.Game.Players.TryAdd(player, new FactorioPlayerData());
            }

            _logger.LogInformation("New players: {players}", string.Join(", ", toAdd));
        }
    }

    void UpdateConnectedPlayers(IReadOnlyCollection<string> connectedPlayers, FactorioData data)
    {
        List<string> connected = [];
        List<string> disconnected = [];

        foreach (string? player in data.Game.Players.Keys)
        {
            bool isOnline = connectedPlayers.Contains(player);

            if (!isOnline && data.Game.Players[player].IsOnline)
            {
                disconnected.Add(player);
            }
            else if (isOnline && !data.Game.Players[player].IsOnline)
            {
                connected.Add(player);
            }

            data.Game.Players[player].IsOnline = isOnline;
        }

        if (connected.Count != 0)
        {
            _logger.LogInformation("Connected: {players}", string.Join(", ", connected));
        }

        if (disconnected.Count != 0)
        {
            _logger.LogInformation("Disconnected: {players}", string.Join(", ", disconnected));
        }
    }
}
