using System.Diagnostics.Metrics;

namespace FactorioSharp.Instrumentation.Meters.Instruments;

abstract class InstrumentBuilderBase<T> where T: struct
{
    protected InstrumentBuilderBase(InstrumentType type, string name, string? unit = null, string? description = null)
    {
        Type = type;
        Name = name;
        Unit = unit;
        Description = description;
    }

    public InstrumentType Type { get; }
    public string Name { get; }
    public string? Unit { get; }
    public string? Description { get; }

    public abstract T Observe();

    public ObservableInstrument<T> Build(Meter meter)
    {
        switch (Type)
        {

            case InstrumentType.Counter:
                return meter.CreateObservableCounter(Name, Observe, Unit, Description);
            case InstrumentType.UpDownCounter:
                return meter.CreateObservableUpDownCounter(Name, Observe, Unit, Description);
            case InstrumentType.Gauge:
                return meter.CreateObservableGauge(Name, Observe, Unit, Description);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
