using FactoriOTel.Configuration.Exporters;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FactoriOTel.Configuration.Yaml;

static class FactoriOTelYamlConfigurationParser
{
    static readonly IDeserializer Deserializer = new DeserializerBuilder().WithTypeConverter(new FactorioStdoutExporterConfigurationYamlTypeConverter())
        .WithNamingConvention(HyphenatedNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    public static FactoriOTelYamlConfiguration? Read(Stream stream)
    {
        using StreamReader reader = new(stream);
        return Deserializer.Deserialize<FactoriOTelYamlConfiguration>(reader);
    }
}

class FactorioStdoutExporterConfigurationYamlTypeConverter : IYamlTypeConverter
{
    public bool Accepts(Type type) => type == typeof(FactorioStdoutExporterConfiguration);

    public object? ReadYaml(IParser parser, Type type)
    {
        if (parser.TryConsume<Scalar>(out _))
        {
            return new FactorioStdoutExporterConfiguration();
        }

        throw new InvalidOperationException("Expected a YAML field with empty value");
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type) => emitter.Emit(new Scalar(""));
}
