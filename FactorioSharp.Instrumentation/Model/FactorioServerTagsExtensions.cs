namespace FactorioSharp.Instrumentation.Model;

static class FactorioServerTagsExtensions
{
    public static void EnrichTags(this FactorioServerData data, Dictionary<string, object?> tags)
    {
        if (data.Name != null)
        {
            tags["factorio.server.name"] = data.Name;
        }

        if (data.FactorioVersion != null)
        {
            tags["factorio.server.version"] = data.FactorioVersion;
        }

        tags["factorio.server.host"] = data.Uri.Host;
        tags["factorio.server.port"] = data.Uri.Port;
    }
}
