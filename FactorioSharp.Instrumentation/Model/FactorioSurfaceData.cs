namespace FactorioSharp.Instrumentation.Model;

class FactorioSurfaceData
{
    public Dictionary<string, uint> Resources { get; set; } = new();
    public Dictionary<uint, FactorioElectronicNetworkData> ElectricNetworks { get; set; } = new();
}

class FactorioElectronicNetworkData
{
    /// <summary>
    ///     Energy flow per entity in J/tick
    /// </summary>
    public FactorioFlowData<double> Flow { get; set; } = new();

    /// <summary>
    ///     Entities count
    /// </summary>
    public Dictionary<string, int> Entities { get; set; } = new();
}
