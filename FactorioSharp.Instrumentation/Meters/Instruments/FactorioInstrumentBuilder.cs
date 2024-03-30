using System.Linq.Expressions;
using FactorioSharp.Instrumentation.Extensions;
using FactorioSharp.Instrumentation.Integration;
using FactorioSharp.Rcon.Model;

namespace FactorioSharp.Instrumentation.Meters.Instruments;

class FactorioInstrumentBuilder<T> : InstrumentBuilder<T> where T: struct
{
    readonly FactorioClient _client;
    readonly Expression<Func<FactorioRconGlobals, T>> _observe;

    public FactorioInstrumentBuilder(
        FactorioClient client,
        InstrumentType type,
        string name,
        Expression<Func<FactorioRconGlobals, T>> observe,
        string? unit = null,
        string? description = null
    ) : base(type, name, null, unit, description)
    {
        _client = client;
        _observe = observe;
        Observe = ObserveImpl;
    }

    public BehaviorOnConnectionLost OnConnectionLost { get; set; } = BehaviorOnConnectionLost.Throw;

    T ObserveImpl()
    {
        return _client.ReadAsync(_observe).RunSync();

        return OnConnectionLost switch
        {
            BehaviorOnConnectionLost.Throw => throw new InvalidOperationException("Connection to server was lost"),
            BehaviorOnConnectionLost.ReturnDefault => default,
            _ => throw new ArgumentOutOfRangeException(nameof(OnConnectionLost), OnConnectionLost, null)
        };
    }
}

enum BehaviorOnConnectionLost
{
    Throw,
    ReturnDefault
}
