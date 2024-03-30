using System.Linq.Expressions;
using FactorioSharp.Instrumentation.Extensions;
using FactorioSharp.Rcon;
using FactorioSharp.Rcon.Model;

namespace FactorioSharp.Instrumentation.Meters.Instruments;

class FactorioInstrument<T> : InstrumentBase<T> where T: struct
{
    readonly FactorioRconClient _client;
    readonly Expression<Func<FactorioRconGlobals, T>> _observe;

    public FactorioInstrument(FactorioRconClient client, string name, Expression<Func<FactorioRconGlobals, T>> observe, string? unit = null, string? description = null) : base(
        name,
        unit,
        description
    )
    {
        _client = client;
        _observe = observe;
    }

    public override T Observe() => _client.ReadAsync(_observe).RunSync();
}
