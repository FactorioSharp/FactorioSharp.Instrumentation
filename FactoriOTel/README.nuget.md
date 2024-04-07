# FactoriOTel

This tool connects to one or many Factorio servers through RCON, reads their data periodically and expose them through OpenTelemetry exporters.

See also the [FactorioSharp.Instrumentation](https://github.com/FactorioSharp/FactorioSharp.Instrumentation/tree/main/FactorioSharp.Instrumentation) nuget to integrate the instrumentation tools to an existing application.

## Getting Started

Get the FactoriOTel application:
- using the .NET CLI: `dotnet tool install --global FactoriOTel`
- or from this repository [releases](https://github.com/FactorioSharp/FactorioSharp.Instrumentation/releases)

Create a `config.yml` file:
```yml
servers:
  - name: my-awesome-server
    rcon-endpoint: localhost:1234
    rcon-password: password

exporters:
  prometheus:
    endpoint: localhost:9090
```

Then run the `FactoriOTel` global command (or the corresponding executable on your platform)   

```
FactoriOTel config.yml
```

## Usage

```
FactoriOTel 0.3.2+ee48cd9bfa2e6847a40f1be1820ca4a82974c351
Copyright (c) 2024 Ismail Bennani
USAGE:
Run using configuration from config.yml:
  FactoriOTel.exe config.yml

  -v, --verbose      (Default: false) Print more information to help diagnose
                     issues with the application
  --help             Display this help screen.
  --version          Display version information.
  config (pos. 0)    Required. Configuration file
```

## Configuration file

See [config.example.yml](https://github.com/FactorioSharp/FactorioSharp.Instrumentation/blob/main/FactoriOTel/config.example.yml).

The file is expected to have the following structure.

```yml
# Servers to read data from.
# At least one server must be configured.
servers:
  [ - <server_config> ... ]

# Exporters that expose the data from the server.
# At least one exporter must be configured.
exporters:
  # Expose an HTTP listener that will listen from prometheus requests.
  # See https://prometheus.io/
  [ prometheus: <prometheus_config> ]

  # Write the signals to the standard output 
  [ stdout: <stdout_config> ]
```

### `<server_config>`

```yml
# The name of the server. 
# Its value will be exported as a `factorio_server_name` tag in all the measurements from the server
name: <string> 

# URI at which the Factorio server exposes its RCON interface.
# Note: the port should be the --rcon-port specified as argument of the Factorio server command
rcon-endpoint: <uri>

# The password specified as --rcon-password argument of the Factorio server command
rcon-password: <string>
```

See [FactorioSharp.Instrumentation](https://github.com/FactorioSharp/FactorioSharp.Instrumentation/tree/main/FactorioSharp.Instrumentation)

### `<prometheus_config>`

```yml
# The endpoint on which the HTTP listener should listen for prometheus requests
endpoint: <uri>

# The path to use for the scraping endpoint
[ scrape-endpoint-path: <string> | default = /metrics ]
```

See [OpenTelemetry.Exporter.Prometheus.HttpListener](https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Exporter.Prometheus.HttpListener/README.md)

### `<stdout_config>`

There are no configuration available for the standard output exporter: it prints the signals to the standard output. \
See [OpenTelemetry.Exporter.Console](https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Exporter.Console/README.md)