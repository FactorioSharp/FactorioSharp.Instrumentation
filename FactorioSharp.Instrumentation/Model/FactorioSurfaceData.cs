namespace FactorioSharp.Instrumentation.Model;

class FactorioSurfaceData
{
    public Dictionary<string, uint> Resources { get; set; } = new();
    public Dictionary<uint, FactorioElectronicNetworkData> ElectricNetworks { get; set; } = new();
}

class FactorioElectronicNetworkData
{
    /// <summary>
    ///     Energy flow in J/tick
    /// </summary>
    public FactorioFlowData<double> Flow { get; set; } = new();

    /// <summary>
    ///     Buffer in J
    /// </summary>
    public Dictionary<string, double> Buffer { get; set; } = new();
}
