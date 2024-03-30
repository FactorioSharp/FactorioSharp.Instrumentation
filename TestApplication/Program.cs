using FactorioSharp.Instrumentation.Meters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

const string serviceName = "Factorio";
const string serviceVersion = "1.1.104";

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddOpenTelemetry()
    .WithMetrics(
        metrics => metrics.ConfigureResource(resource => resource.AddService(serviceName, serviceVersion)).AddFactorioInstrumentation("127.0.0.1", "password").AddConsoleExporter()
    );

IHost app = builder.Build();

app.Run();
