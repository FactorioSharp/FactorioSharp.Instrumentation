using System.Text.Json;
using CommandLine;
using CommandLine.Text;
using FactoriOTel.CommandLine;
using FactoriOTel.Configuration;
using FactoriOTel.Configuration.Exporters;
using FactoriOTel.Configuration.Validation;
using FactoriOTel.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Serilog;
using Serilog.Extensions.Hosting;

string applicationName = typeof(Program).Assembly.GetName().Name!;
Version applicationVersion = typeof(Program).Assembly.GetName().Version!;

Parser parser = new(with => with.HelpWriter = null);
ParserResult<FactoriOTelArguments> parserResult = parser.ParseArguments<FactoriOTelArguments>(args);
parserResult.WithParsed(arguments => Run(args, arguments)).WithNotParsed(_ => DisplayHelp(parserResult));

return;

void Run(string[] args, FactoriOTelArguments arguments)
{
    JsonSerializerOptions jsonSerializerOptions = new();
    Log.Logger = ConfigureLogger(arguments);

    Log.Logger.Debug("CLI arguments: {config}", JsonSerializer.Serialize(arguments, SourceGenerationContext.Default.FactoriOTelArguments));

    FactoriOTelConfiguration configuration = FactoriOTelConfigurationParser.FromYaml(arguments.ConfigurationFile);

    Log.Logger.Debug("Configuration: {config}", JsonSerializer.Serialize(configuration, SourceGenerationContext.Default.FactoriOTelConfiguration));

    FactoriOTelValidationResult configurationValidationResult = FactoriOTelValidator.Validate(configuration);

    if (!configurationValidationResult.IsValid)
    {
        Log.Logger.Error("Bad configuration, see below.{errors}", string.Join("", configurationValidationResult.Errors.Select(e => $"{Environment.NewLine}\t- {e}")));
        Environment.Exit(1);
    }

    HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddSerilog((services, lc) => lc.ReadFrom.Services(services).Enrich.FromLogContext().WriteTo.Console());

    builder.Services.AddOpenTelemetry()
        .WithMetrics(
            metricsBuilder =>
            {
                metricsBuilder.ConfigureResource(resource => resource.AddService(applicationName, applicationVersion.ToString()));

                foreach (FactorioServerConfiguration serverConfiguration in configuration.Servers)
                {
                    metricsBuilder.ConfigureServer(serverConfiguration);
                }

                foreach (FactorioExporterConfiguration exporterConfiguration in configuration.Exporters)
                {
                    metricsBuilder.ConfigureExporter(exporterConfiguration);
                }
            }
        );

    IHost app = builder.Build();

    app.Run();
}

void DisplayHelp<T>(ParserResult<T> result)
{
    HelpText? helpText = HelpText.AutoBuild(
        result,
        h =>
        {
            h.AdditionalNewLineAfterOption = false;
            h.Copyright = "Copyright (c) 2024 Ismail Bennani";
            return HelpText.DefaultParsingErrorsHandler(result, h);
        },
        e => e
    );

    Console.WriteLine(helpText);
}

ReloadableLogger ConfigureLogger(FactoriOTelArguments arguments)
{
    LoggerConfiguration loggerConfiguration = new LoggerConfiguration().Enrich.FromLogContext().WriteTo.Console();

    if (arguments.Verbose)
    {
        loggerConfiguration.MinimumLevel.Debug();
    }

    return loggerConfiguration.CreateBootstrapLogger();
}
