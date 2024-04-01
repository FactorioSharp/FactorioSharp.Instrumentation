using System.Collections.Concurrent;

namespace FactorioSharp.Instrumentation.Model;

class FactorioFlowData<TValue>
{
    public ConcurrentDictionary<string, TValue> Inputs { get; } = new();
    public ConcurrentDictionary<string, TValue> Outputs { get; } = new();
}
