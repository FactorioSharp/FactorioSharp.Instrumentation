using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FactorioSharp.Instrumentation.Meters;

public class FactorioMeterOptions
{
    /// <summary>
    ///     If set to true, the value of <see cref="MeasuredForces" /> will be ignored and all the available forces will be measured instead
    /// </summary>
    public bool MeasureAllForces { get; set; } = false;

    /// <summary>
    ///     The forces that should be measured
    /// </summary>
    public HashSet<string> MeasuredForces { get; } = ["player"];

    /// <summary>
    ///     If set to true, the value of <see cref="MeasuredSurfaces" /> will be ignored and all the available surfaces will be measured instead
    /// </summary>
    public bool MeasureAllSurfaces { get; set; } = true;

    /// <summary>
    ///     The surfaces that should be measured
    /// </summary>
    public HashSet<string> MeasuredSurfaces { get; } = [];

    /// <summary>
    ///     If set to true, the value of <see cref="MeasuredItems" /> will be ignored and all the available items will be measured instead
    /// </summary>
    public bool MeasureAllItems { get; set; } = true;

    /// <summary>
    ///     The items that should be measured
    /// </summary>
    public HashSet<string> MeasuredItems { get; } = [];

    /// <summary>
    ///     If set to true, the value of <see cref="MeasuredFluids" /> will be ignored and all the available fluids will be measured instead
    /// </summary>
    public bool MeasureAllFluids { get; set; } = true;

    /// <summary>
    ///     The fluids that should be measured
    /// </summary>
    public HashSet<string> MeasuredFluids { get; } = [];

    public ILoggerFactory LoggerFactory { get; set; } = NullLoggerFactory.Instance;
}
