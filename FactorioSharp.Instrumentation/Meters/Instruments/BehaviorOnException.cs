namespace FactorioSharp.Instrumentation.Meters.Instruments;

enum BehaviorOnException
{
    /// <summary>
    ///     Will throw the exception.
    ///     OpenTelemetry will handle the exception internally, it seems that it means keeping the old value as is.
    /// </summary>
    Throw,

    /// <summary>
    ///     Return the default value for the corresponding type.
    ///     This uses the C# `default` symbol.
    /// </summary>
    ReturnDefault
}
