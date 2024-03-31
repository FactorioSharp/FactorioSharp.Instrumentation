namespace FactorioSharp.Instrumentation.Integration.Model;

class FactorioProductionData
{
    public FactorioFlowData<ulong> Item { get; } = new();
    public FactorioFlowData<double> Fluid { get; } = new();
}