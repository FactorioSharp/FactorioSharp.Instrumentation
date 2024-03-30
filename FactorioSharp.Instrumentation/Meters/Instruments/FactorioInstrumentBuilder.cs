using System.Linq.Expressions;
using FactorioSharp.Instrumentation.Extensions;
using FactorioSharp.Rcon;
using FactorioSharp.Rcon.Model;

namespace FactorioSharp.Instrumentation.Meters.Instruments;

class FactorioInstrumentBuilder<T> : InstrumentBuilderBase<T> where T: struct
{
    readonly FactorioRconClient _client;
    readonly Expression<Func<FactorioRconGlobals, T>> _observe;

    public FactorioInstrumentBuilder(
        FactorioRconClient client,
        InstrumentType type,
        string name,
        Expression<Func<FactorioRconGlobals, T>> observe,
        string? unit = null,
        string? description = null
    ) : base(type, name, unit, description)
    {
        _client = client;
        _observe = observe;
    }

    public override T Observe() => _client.ReadAsync(_observe).RunSync();
}
