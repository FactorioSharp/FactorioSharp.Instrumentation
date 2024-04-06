using FactorioSharp.Instrumentation.Scheduling;

namespace FactorioSharp.Instrumentation;

class FactorioInstrumentationConstants
{
    public static readonly string MeterName = typeof(FactorioInstrumentationBackgroundWorker).Assembly.GetName().Name!;
    public static readonly string MeterVersion = typeof(FactorioInstrumentationBackgroundWorker).Assembly.GetName().Version!.ToString();
}
