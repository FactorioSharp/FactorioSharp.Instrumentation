namespace FactorioSharp.Instrumentation.Integration;

/// <summary>
///     Internal equivalent of <see cref="FactorioInstrumentationOptions" />.
///     The values configured by the user on the other object are used to fill this one which will be used internally during the whole execution.
/// </summary>
public class FactorioInstrumentationOptionsInternal
{
    public IReadOnlyCollection<string> Forces { get; set; } = Array.Empty<string>();
    public IReadOnlyCollection<string> Items { get; set; } = Array.Empty<string>();
    public IReadOnlyCollection<string> Fluids { get; set; } = Array.Empty<string>();
}
