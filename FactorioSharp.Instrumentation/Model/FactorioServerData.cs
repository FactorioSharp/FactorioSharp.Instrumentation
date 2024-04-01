namespace FactorioSharp.Instrumentation.Model;

public class FactorioServerData
{
    public FactorioServerData(Uri uri, string? name)
    {
        Uri = uri;
        Name = name;
    }

    public Uri Uri { get; set; }
    public string? Name { get; set; }
    public string? FactorioVersion { get; set; }
    public bool IsConnected { get; set; }
    public Dictionary<string, string> Mods { get; set; } = new();
    public string[] Players { get; set; } = Array.Empty<string>();
    public string[] ConnectedPlayers { get; set; } = Array.Empty<string>();
}
