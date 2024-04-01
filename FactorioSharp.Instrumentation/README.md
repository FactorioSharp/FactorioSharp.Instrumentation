# FactorioSharp.Instrumentation

Collect data from a Factorio server through an RCON connection and define emit them as metrics.

In order to keep the number of requests per second manageable, the collection of data from the server is rate-limited: in addition to specifying refresh rates on the different quantities that are measured, there is a master rate-limit that restricts the number of requests per second performed on the server.  
The orchestration is performed by a background service that must be added to the host application.

Note: Managing activities for the commands that are executed on the RCON is a **WIP**.

This project has been created with OpenTelemetry in mind, see the `FactorioSharp.Instrumentation.OpenTelemetry` project.