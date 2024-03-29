using FactorioSharp.Instrumentation.Meters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Serilog;
using Serilog.Core;
using Serilog.Extensions.Logging;

const string serviceName = "Factorio";
const string serviceVersion = "1.1.104";

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

LoggerConfiguration loggerConfiguration = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Console();
Logger logger = loggerConfiguration.CreateLogger();
Log.Logger = logger;

SerilogLoggerFactory factory = new(logger);

try
{
    Log.Information("Starting web application");

    ResourceBuilder resourceBuilder = ResourceBuilder.CreateDefault().AddService(serviceName, serviceVersion);

    builder.Logging.ClearProviders();
    builder.Logging.AddSerilog();

    builder.Services.AddOpenTelemetry()
        .WithMetrics(metrics => metrics.SetResourceBuilder(resourceBuilder).AddFactorioInstrumentation("127.0.0.1", "password", loggerFactory: factory).AddConsoleExporter());

    IHost app = builder.Build();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
