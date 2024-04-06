using FactorioSharp.Instrumentation.Meters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

const string serviceName = "Factorio";

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddOpenTelemetry()
    .WithMetrics(
        metricsBuilder => metricsBuilder.ConfigureResource(resource => resource.AddService(serviceName))
            .AddFactorioInstrumentation(
                options =>
                {
                    options.Server.Name = "Grafcity";
                    options.Server.Uri = new Uri("http://localhost:27015");
                    options.Server.RconPassword = "password";
                }
            )
            .AddPrometheusHttpListener(options => options.UriPrefixes = ["http://localhost:9184/"])
    );

IHost app = builder.Build();

app.Run();
