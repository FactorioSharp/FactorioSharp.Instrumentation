# Export OpenTelemetry signals from a Factorio Server

This repository has two parts:
- the [Factorio.Instrumentation](https://github.com/FactorioSharp/FactorioSharp.Instrumentation/tree/main/FactorioSharp.Instrumentation) library: implements the collection of data from Factorio servers through an RCON connection, and meters to expose them
- [FactoriOTel](https://github.com/FactorioSharp/FactorioSharp.Instrumentation/tree/main/FactoriOTel): a tool that uses the instrumentation library above and exports the data through various OpenTelemetry exporters such as Prometheus.

The goal of this solution is to make Factorio observable through the network. The measurements can be used with any tool that is integrated with the OpenTelemetry ecosystem. 

For example, here is a Grafana dashboard in an early game of vanilla Factorio. The metrics are collected by FactoriOTel and exported to Prometheus, then to Grafana.

![Grafana dashboard displaying the amount of iron (ore and plate) and copper (ore and plate) that the player has available](imgs/grafana-dashboard.png)