using System.Linq.Expressions;
using FactorioSharp.Instrumentation.Extensions;
using FactorioSharp.Instrumentation.Integration;
using FactorioSharp.Rcon.Model;

namespace FactorioSharp.Instrumentation.Meters.Instruments;

class FactorioInstrumentBuilder<T> : InstrumentBuilder<T> where T: struct
{
    public FactorioInstrumentBuilder(
        FactorioClient client,
        InstrumentType type,
        string name,
        Expression<Func<FactorioRconGlobals, T>> observe,
        string? unit = null,
        string? description = null
    ) : base(type, name, () => Observe(client, observe), unit, description)
    {
    }

    static T Observe(FactorioClient client, Expression<Func<FactorioRconGlobals, T>> observe) => client.ReadAsync(observe).RunSync();
}
