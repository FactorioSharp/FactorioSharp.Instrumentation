namespace FactorioSharp.Instrumentation.Integration.Model;

class FactorioFlowData<TValue>
{
    public Dictionary<string, TValue> Inputs { get; } = new();
    public Dictionary<string, TValue> Outputs { get; } = new();
}