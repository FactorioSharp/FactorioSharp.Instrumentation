# Export OpenTelemetry signals from a Factorio Server

This repository has two parts:
- the [Factorio.Instrumentation](https://github.com/FactorioSharp/FactorioSharp.Instrumentation/tree/main/FactorioSharp.Instrumentation) library: implements the collection of data from Factorio servers through an RCON connection, and meters to expose them
- [FactoriOTel](https://github.com/FactorioSharp/FactorioSharp.Instrumentation/tree/main/FactoriOTel): a tool that uses the instrumentation library above and exports the data through various OpenTelemetry exporters such as Prometheus.

The goal of this solution is to make Factorio observable through the network. The measurements can be used with any tool that is integrated with the OpenTelemetry ecosystem. 

## Example

Here is a Grafana dashboard in an early game of vanilla Factorio. The metrics are collected by FactoriOTel and exported to Prometheus, then to Grafana.

![Grafana dashboard displaying the amount of iron (ore and plate) and copper (ore and plate) that the player has available](imgs/grafana-dashboard.png)

## Metrics and tags 

The instrumentation library and the application export several metrics. Each measure comes with a set of tags that adds context, some of those tags are added for all metrics.

**Global tags**
- `factorio.server.name`: the name of the server on which the data has been measured
- `factorio.server.version`: the version of factorio that is running on the server. This is the version of the `base` mod
- `factorio.server.host`: the host used to access the server
- `factorio.server.port`: the port used for the RCON connection

**Metrics**

Below is an exhaustive list of all the metrics exposed by the library and the application. Each metrics is described along with the tags that its measurements have.
For example, the `factorio.game.electricity.input` will receive one measurement per type of entity consuming electricity per interval. The measurements will have different tags identifying the entities that correspond to the measurement.

- Server
  - `factorio.server.status`: 1 - server is up, 0 - server is down

- General game data
  - `factorio.game.player`: total number of players on the server. This include the offline players. See [LuaGameScript.players](https://lua-api.factorio.com/latest/classes/LuaGameScript.html#players)
  - `factorio.game.player.connected`: the number of players that are currently connected to the server [LuaGameScript.connected_players](https://lua-api.factorio.com/latest/classes/LuaGameScript.html#connected_players)
  - `factorio.game.tick`: the current map tick. See [LuaGameScript.tick](https://lua-api.factorio.com/latest/classes/LuaGameScript.html#tick)
  - `factorio.game.tick.played`: the number of ticks since the game was created. See [LuaGameScript.ticks_played](https://lua-api.factorio.com/latest/classes/LuaGameScript.html#ticks_played)
  - `factorio.game.tick.paused`: 1 - server is paused, 0 - server is running. See [LuaGameScript.ticks_played](https://lua-api.factorio.com/latest/classes/LuaGameScript.html#ticks_played)
  - `factorio.game.tick.speed_ratio`: the current speed multiplier. See [LuaGameScript.tick_paused](https://lua-api.factorio.com/latest/classes/LuaGameScript.html#tick_paused)

- Mineable resources
  - `factorio.game.mineable_resource`: the cumulated amount of each mineable entity that has been discovered on the map (e.g. ore). See [LuaSurface.get_resource_counts()](https://lua-api.factorio.com/latest/classes/LuaSurface.html#get_resource_counts)
    - `factorio.entity`: the name of the entity that can be mined
    - `factorio.surface`: the name of the surface on which the resource is located
    - `factorio.resource.category`: the resource category

- Items
  - `factorio.game.item.input`: the amount of items that has been produced (all time). See [LuaForce.item_production_statistics](https://lua-api.factorio.com/latest/classes/LuaForce.html#item_production_statistics)
    - `factorio.item`: the name of the item
    - `factorio.force`: the force that has produced the item
  - `factorio.game.item.output`: the amount of items that has been consumed (all time). See [LuaForce.item_production_statistics](https://lua-api.factorio.com/latest/classes/LuaForce.html#item_production_statistics)
    - `factorio.item`: the name of the item
    - `factorio.force`: the force that has consumed the item
    - 
- Fluids
  - `factorio.game.fluid.input`: the amount of fluids that has been produced (all time). See [LuaForce.fluid_production_statistics](https://lua-api.factorio.com/latest/classes/LuaForce.html#fluid_production_statistics)
    - `factorio.fluid`: the name of the fluid
    - `factorio.force`: the force that has produced the fluid
  - `factorio.game.fluid.output`: the amount of fluids that has been consumed (all time). See [LuaForce.fluid_production_statistics](https://lua-api.factorio.com/latest/classes/LuaForce.html#fluid_production_statistics)
    - `factorio.fluid`: the name of the fluid
    - `factorio.force`: the force that has consumed the fluid

- Electricity
  - `factorio.game.electricity.input` (J): The amount of electricity that has been consumed (all time). See [LuaEntity.electric_network_statistics](https://lua-api.factorio.com/latest/classes/LuaEntity.html#electric_network_statistics)
    - `factorio.entity`: the entity that consumed the energy
    - `factorio.entity.type`: one of `Sink`, `Source` or `Accumulator`
    - `factorio.surface`: the surface on which the energy has been consumed
    - `factorio.network`: the id of the network on which the energy has been consumed
  - `factorio.game.electricity.output` (J): The amount of electricity that has been produced (all time). See [LuaEntity.electric_network_statistics](https://lua-api.factorio.com/latest/classes/LuaEntity.html#electric_network_statistics)
    - `factorio.entity`: the entity that produced the energy
    - `factorio.entity.type`: one of `Sink`, `Source` or `Accumulator`
    - `factorio.surface`: the surface on which the energy has been produced
    - `factorio.network`: the id of the network on which the energy has been produced
  - `factorio.game.electricity.entity`: The number of entities that consume, produce or buffer energy. See [LuaEntity.get_filtered_entity_prototypes()](https://lua-api.factorio.com/latest/classes/LuaGameScript.html#get_filtered_entity_prototypes)
    - `factorio.entity`: the entity name
    - `factorio.entity.type`: one of `Sink`, `Source` or `Accumulator`
    - `factorio.surface`: the surface on which the entities are placed
    - `factorio.network`: the id of the network to which the entities are connected
  - `factorio.game.electricity.input.min_usage` (J/tick): The minimum amount of energy consumed by entities, even at rest. See [LuaElectricEnergySourcePrototype.drain](https://lua-api.factorio.com/latest/classes/LuaElectricEnergySourcePrototype.html#drain)
    - `factorio.entity`: the entity that drains the energy
    - `factorio.entity.type`: one of `Sink`, `Source` or `Accumulator`
    - `factorio.surface`: the surface on which the energy is drained
    - `factorio.network`: the id of the network on which the energy is drained
  - `factorio.game.electricity.input.max_usage` (J/tick): The theoretical maximum amount of energy consumed by entities. See [LuaElectricEnergySourcePrototype.max_energy_usage](https://lua-api.factorio.com/latest/classes/LuaEntityPrototype.html#max_energy_usage)
    - `factorio.entity`: the entity that consumes the energy
    - `factorio.entity.type`: one of `Sink`, `Source` or `Accumulator`
    - `factorio.surface`: the surface on which the energy is consumed
    - `factorio.network`: the id of the network on which the energy is consumed
  - `factorio.game.electricity.output.max_production` (J/tick): The theoretical maximum amount of energy produced by entities. See [LuaElectricEnergySourcePrototype.max_energy_production](https://lua-api.factorio.com/latest/classes/LuaEntityPrototype.html#max_energy_production)
    - `factorio.entity`: the entity that produces the energy
    - `factorio.entity.type`: one of `Sink`, `Source` or `Accumulator`
    - `factorio.surface`: the surface on which the energy is produced
    - `factorio.network`: the id of the network on which the energy is produced
  - `factorio.game.electricity.buffer.max_capacity` (J): The maximum amount of energy that can be buffered by entities. See [LuaElectricEnergySourcePrototype.buffer_capacity](https://lua-api.factorio.com/latest/classes/LuaElectricEnergySourcePrototype.html#buffer_capacity)
    - `factorio.entity`: the entity that buffers the energy
    - `factorio.entity.type`: one of `Sink`, `Source` or `Accumulator`
    - `factorio.surface`: the surface on which the energy is buffered
    - `factorio.network`: the id of the network on which the energy is buffered