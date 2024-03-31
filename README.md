# FactorioSharp.Instrumentation

Collect data from a Factorio server through an RCON connection and define meters that read the data.
The data can then be fed to any OpenTelemetry exporter.

In order to keep the number of requests per second manageable, the collection of data from the server is rate-limited: in addition to specifying refresh rates on the different quantities that are measured, there is a master rate-limit that restricts the number of requests per second performed on the server.  
The orchestration is performed by a background service that must be added to the host application.

## Quick start

The RCON interface must be enabled when running factorio. It must also be secured using a password.
Add the following parameters to the factorio launch command: `--rcon-port 27015 --rcon-password factory`

For example
```
bin\x64\factorio.exe --start-server saves/save.zip --rcon-port 27015 --rcon-password password
```

You should see a log message in the factorio console that looks like:

```
Info RemoteCommandProcessor.cpp:133: Starting RCON interface at IP ADDR:({0.0.0.0:27015})
```

The instrumentation can then be configured using the provided extension methods

### Using the OpenTelemetry SDK

```
using FactorioSharp.Instrumentation.Meters;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

const string serviceName = "Factorio";
const string serviceVersion = "1.1.104";

using MeterProvider MeterProvider = Sdk.CreateMeterProviderBuilder()
    .ConfigureResource(resource => resource.AddService(serviceName, serviceVersion))
    .AddFactorioInstrumentation("127.0.0.1", "password")
    .AddConsoleExporter()
    .Build();
```

### Using the Hosting extensions

```
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
            .AddConsoleExporter()
    );

IHost app = builder.Build();

app.Run();

```