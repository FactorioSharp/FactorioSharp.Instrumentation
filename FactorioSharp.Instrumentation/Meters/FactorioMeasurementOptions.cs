namespace FactorioSharp.Instrumentation.Meters;

/// <summary>
///     Options regarding the collection of data from the factorio server
/// </summary>
public class FactorioMeasurementOptions
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

    /// <summary>
    ///     Rate at which data must be read from the factorio server.
    ///     The higher this value, the more intensive the data collection will be on the server. <br />
    ///     Defaults to 15 seconds.
    /// </summary>
    /// <remarks>
    ///     There is no point in having a value higher than the refresh interval of the tool that consumes the telemetry. For example if prometheus is collecting the data at
    ///     a scrape_interval of 15s, this interval should also be 15s.
    /// </remarks>
    public TimeSpan ObservationInterval { get; set; } = TimeSpan.FromSeconds(15);

    /// <summary>
    ///     Interval of time between two connection attempts: when the server is lost, how often should we try to reconnect. <br />
    ///     Defaults to 1 minute.
    /// </summary>
    /// <remarks>
    ///     There is no point in having a value higher than the refresh interval of the tool that consumes the telemetry. For example if prometheus is collecting the data at
    ///     a scrape_interval of 15s, this interval should also be 15s.
    /// </remarks>
    public TimeSpan ReconnectionInterval { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    ///     If true, commands on the factorio server will be spread over one period instead of being performed at once. <br />
    ///     Defaults to true.
    /// </summary>
    public bool SpreadMeasurements { get; set; } = true;
}
