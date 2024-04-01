namespace FactorioSharp.Instrumentation.Model;

class FactorioData
{
    public FactorioData(Uri uri, string? name)
    {
        Server = new FactorioServerData(uri, name);
    }

    public FactorioServerData Server { get; }
    public FactorioGameData Game { get; } = new();
}
