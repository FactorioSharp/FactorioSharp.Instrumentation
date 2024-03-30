using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;

namespace FactorioSharp.Instrumentation.Meters.Instruments;

class InstrumentBuilder<T> where T: struct
{
    protected Func<T>? Observe { get; set; }

    protected InstrumentBuilder(InstrumentType type, string name, Func<T>? observe = null, string? unit = null, string? description = null)
    {
        Observe = observe;
        Type = type;
        Name = name;
        Unit = unit;
        Description = description;
    }

    public InstrumentType Type { get; }
    public string Name { get; }
    public string? Unit { get; }
    public string? Description { get; }

    /// <summary>
    ///     If provided, this logger will be used to log exceptions that happen during the measurement
    /// </summary>
    public ILogger? Log { get; set; }

    /// <summary>
    ///     Define how to handle an exception during measurement. The exception will ALWAYS be logged if <see cref="Log" /> is provided, then it can be:
    ///     <list type="bullet">
    ///         <item>Propagated (default): it will be thrown again which will prevent any value from being measured</item>
    ///         <item>Ignored: the result of the measure will be the default value for the type</item>
    ///     </list>
    /// </summary>
    public BehaviorOnException OnException { get; set; } = BehaviorOnException.Throw;

    public ObservableInstrument<T> Build(Meter meter)
    {
        switch (Type)
        {

            case InstrumentType.Counter:
                return meter.CreateObservableCounter(Name, ObserveInternal, Unit, Description);
            case InstrumentType.UpDownCounter:
                return meter.CreateObservableUpDownCounter(Name, ObserveInternal, Unit, Description);
            case InstrumentType.Gauge:
                return meter.CreateObservableGauge(Name, ObserveInternal, Unit, Description);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    T ObserveInternal()
    {
        try
        {
            if (Observe == null)
            {
                throw new ArgumentNullException(nameof(Observe), "Observation method not defined");
            }

            return Observe();
        }
        catch (Exception exn)
        {
            Log?.LogError(exn, "An error occured while measuring '{name}'.", Name);

            switch (OnException)
            {
                case BehaviorOnException.Throw:
                    throw;
                case BehaviorOnException.ReturnDefault:
                    return default;
                default:
                    throw new ArgumentOutOfRangeException(nameof(OnException), OnException, null);
            }
        }
    }
}
