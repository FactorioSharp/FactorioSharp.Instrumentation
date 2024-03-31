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

class FactorioForceData
{
    public FactorioProductionData Production { get; } = new();
}

class FactorioProductionData
{
    public FactorioFlowData<ulong> Item { get; } = new();
    public FactorioFlowData<double> Fluid { get; } = new();
}

class FactorioFlowData<TValue>
{
    public Dictionary<string, TValue> Inputs { get; } = new();
    public Dictionary<string, TValue> Outputs { get; } = new();
}
