using System.Collections.Concurrent;

namespace FactorioSharp.Instrumentation.Model;

/// <summary>
///     Data collected from the Factorio server.
///     This data is used as a cache by the jobs.
/// </summary>
class FactorioGameData
{
    public FactorioGameTimeData Time { get; } = new();

    public Dictionary<string, MineableResource> MineableResources { get; set; } = new();
    public Dictionary<string, ElectricEntity> ElectricEntities { get; set; } = new();

    public ConcurrentDictionary<string, FactorioPlayerData> Players { get; } = new();
    public ConcurrentDictionary<string, FactorioModData> Mods { get; } = new();
    public ConcurrentDictionary<string, FactorioSurfaceData> Surfaces { get; } = new();
    public ConcurrentDictionary<string, FactorioForceData> Forces { get; } = new();
}
