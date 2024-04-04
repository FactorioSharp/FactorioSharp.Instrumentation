namespace FactorioSharp.Instrumentation.Model;

class FactorioSurfaceData
{
    public Dictionary<string, uint> Resources { get; set; } = new();
    public Dictionary<uint, FactorioElectronicNetworkData> ElectricNetworks { get; set; } = new();
}

class FactorioElectronicNetworkData
{
    public FactorioFlowData<double> Flow { get; set; } = new();
    public Dictionary<string, double> Buffer { get; set; } = new();
}
