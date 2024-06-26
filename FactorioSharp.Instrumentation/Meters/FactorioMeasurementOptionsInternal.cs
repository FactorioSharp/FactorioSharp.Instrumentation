﻿namespace FactorioSharp.Instrumentation.Meters;

/// <summary>
///     Internal equivalent of <see cref="FactorioMeasurementOptions" />.
///     This type is filled using the values configured by the user and values from the server.
///     For example, the content of MeasuredItems is the intersection between the items requested by the user and the ones provided by the server.
/// </summary>
class FactorioMeasurementOptionsInternal
{
    public FactorioMeasurementOptionsInternal(FactorioMeasurementOptions original)
    {
        Original = original;

        MeasuredForces = original.MeasuredForces.ToArray();
        MeasuredSurfaces = original.MeasuredSurfaces.ToArray();
        MeasuredItems = original.MeasuredItems.ToArray();
        MeasuredFluids = original.MeasuredFluids.ToArray();
    }

    public IReadOnlyCollection<string> MeasuredForces { get; set; }
    public IReadOnlyCollection<string> MeasuredSurfaces { get; set; }
    public IReadOnlyCollection<string> MeasuredItems { get; set; }
    public IReadOnlyCollection<string> MeasuredFluids { get; set; }

    /// <summary>
    ///     Original option object, the one provided by the user
    /// </summary>
    public FactorioMeasurementOptions Original { get; }
}
