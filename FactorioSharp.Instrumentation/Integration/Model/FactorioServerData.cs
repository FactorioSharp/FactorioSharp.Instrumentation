namespace FactorioSharp.Instrumentation.Integration.Model;

/// <summary>
///     Data collected from the Factorio server.
///     This data is used as a cache by the jobs.
/// </summary>
class FactorioServerData
{
    public bool Status { get; set; }
    public string[] ItemPrototypes { get; set; } = Array.Empty<string>();

    public Dictionary<string, FactorioForceData> Forces { get; } = new();
}
