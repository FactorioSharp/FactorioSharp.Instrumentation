using CommandLine;
using CommandLine.Text;

namespace FactoriOTel.CommandLine;

/// <summary>
///     CLI arguments
/// </summary>
public class FactoriOTelArguments
{
    /// <summary>
    ///     The configuration file to use
    /// </summary>
    [Value(0, MetaName = "config", HelpText = "Configuration file", Required = true)]
    public required string ConfigurationFile { get; set; }

    /// <summary>
    ///     Should we print more information ?
    /// </summary>
    [Option('v', "verbose", Default = false, HelpText = "Print more information to help diagnose issues with the application")]
    public bool Verbose { get; set; }

    /// <summary>
    ///     Usages
    /// </summary>
    [Usage(ApplicationAlias = "FactoriOTel.exe")]
    public static IEnumerable<Example> Examples =>
    [
        new Example("Run using configuration from config.yml", new FactoriOTelArguments { ConfigurationFile = "config.yml" })
    ];
}
