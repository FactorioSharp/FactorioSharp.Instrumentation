using System.Collections.Concurrent;

namespace FactorioSharp.Instrumentation.Model;

/// <summary>
///     Data collected from the Factorio server.
///     This data is used as a cache by the jobs.
/// </summary>
class FactorioGameData
{
    public MineableResource[] MineableResources { get; set; } = Array.Empty<MineableResource>();

    public ConcurrentDictionary<string, FactorioSurfaceData> Surfaces { get; } = new();
    public ConcurrentDictionary<string, FactorioForceData> Forces { get; } = new();
}
