namespace FactorioSharp.Instrumentation.Model;

class FactorioProductionData
{
    public FactorioFlowData<ulong> Item { get; } = new();
    public FactorioFlowData<double> Fluid { get; } = new();
}
