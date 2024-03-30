using FactorioSharp.Instrumentation.Meters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

const string serviceName = "Factorio";
const string serviceVersion = "1.1.104";

ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddSimpleConsole());

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddOpenTelemetry()
    .WithMetrics(
        metrics => metrics.ConfigureResource(resource => resource.AddService(serviceName, serviceVersion))
            .AddFactorioInstrumentation("127.0.0.1", "password", loggerFactory: loggerFactory)
            .AddConsoleExporter((_, options) => options.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 1000)
    );

IHost app = builder.Build();

app.Run();
