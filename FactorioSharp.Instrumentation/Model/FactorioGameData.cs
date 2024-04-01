namespace FactorioSharp.Instrumentation.Model;

/// <summary>
///     Data collected from the Factorio server.
///     This data is used as a cache by the jobs.
/// </summary>
class FactorioGameData
{
    public string[] ItemPrototypes { get; set; } = Array.Empty<string>();
    public Dictionary<string, FactorioForceData> Forces { get; } = new();
}
