namespace FactorioSharp.Instrumentation.Model;

class FactorioServerData
{
    public FactorioServerData(Uri uri, string? name)
    {
        Uri = uri;
        Name = name;
    }

    public Uri Uri { get; set; }
    public string? Name { get; set; }
    public string? FactorioVersion { get; set; }
    public bool IsUp { get; set; }
}
