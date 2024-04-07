namespace FactoriOTel.Configuration;

/// <summary>
///     Configuration of a Factorio server to read data from
/// </summary>
public class FactorioServerConfiguration
{
    /// <summary>
    ///     The name of the server. <br />
    ///     This value will be used as <c>factorio_server_name</c> tag in the exported metrics.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    ///     The endpoint where the Factorio server exposes the RCON connection. <br />
    ///     This value should be a valid URI, e.g. <c>http://localhost:27015</c>.
    ///     The port should be the RCON port, i.e. the value passed as <c>--rcon-password</c> option to the Factorio <c>--start-server</c> command.
    /// </summary>
    public required string RconEndpoint { get; set; }

    /// <summary>
    ///     The RCON password of the server.
    ///     This is the value passed as <c>--rcon-password</c> option to the Factorio <c>--start-server</c> command.
    /// </summary>
    public required string RconPassword { get; set; }
}
