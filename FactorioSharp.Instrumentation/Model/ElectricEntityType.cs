namespace FactorioSharp.Instrumentation.Model;

[Flags]
enum ElectricEntityType
{
    None = 0,
    Sink = 1,
    Source = 2,
    Accumulator = 4
}