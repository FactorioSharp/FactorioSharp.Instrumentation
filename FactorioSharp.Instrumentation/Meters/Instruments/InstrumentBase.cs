namespace FactorioSharp.Instrumentation.Meters.Instruments;

abstract class InstrumentBase<T> where T: struct
{
    protected InstrumentBase(string name, string? unit = null, string? description = null)
    {
        Name = name;
        Unit = unit;
        Description = description;
    }

    public string Name { get; }
    public string? Unit { get; }
    public string? Description { get; }
    public bool Enabled { get; private set; } = true;

    public void Enable() => Enabled = true;

    public void Disable() => Enabled = false;

    public abstract T Observe();
}
