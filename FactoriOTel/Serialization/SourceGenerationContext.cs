using System.Text.Json.Serialization;
using FactoriOTel.CommandLine;
using FactoriOTel.Configuration;

namespace FactoriOTel.Serialization;

[JsonSourceGenerationOptions]
[JsonSerializable(typeof(FactoriOTelArguments))]
[JsonSerializable(typeof(FactoriOTelConfiguration))]
partial class SourceGenerationContext : JsonSerializerContext
{
}
