namespace FactorioSharp.Instrumentation;

/// <summary>
///     Options regarding the factorio server to read data from
/// </summary>
public class FactorioServerOptions
{
    /// <summary>
    ///     The URI of the server
    /// </summary>
    public Uri? Uri { get; set; }

    /// <summary>
    ///     The password of the RCON connection
    /// </summary>
    public string? RconPassword { get; set; }

    /// <summary>
    ///     The name of the server.
    ///     If set, this name will be added as a tag named <c>factorio_server_name</c> on all metrics.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     If false, the commands will be executed on the factorio server using the <c>/c</c> command instead of the silent <c>/sc</c>.
    ///     Setting this to false can help diagnose issues. <br />
    ///     Defaults to true.
    /// </summary>
    public bool SilentCommands { get; set; } = true;
}
