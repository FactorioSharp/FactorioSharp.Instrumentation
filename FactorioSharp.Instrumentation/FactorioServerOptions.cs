namespace FactorioSharp.Instrumentation;

/// <summary>
/// Options regarding the factorio server to read data from
/// </summary>
public class FactorioServerOptions
{
    /// <summary>
    /// The URI of the server
    /// </summary>
    public Uri? Uri { get; set; }
    
    /// <summary>
    /// The password of the RCON connection
    /// </summary>
    public string? RconPassword { get; set; }
    
    /// <summary>
    /// The name of the server
    /// </summary>
    public string? Name { get; set; }
}