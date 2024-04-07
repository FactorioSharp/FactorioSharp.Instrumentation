using System.Text.Json;
using FactorioSharp.Instrumentation.Meters;
using FactorioSharp.Instrumentation.Model;
using FactorioSharp.Instrumentation.Scheduling;
using FactorioSharp.Rcon;
using Microsoft.Extensions.Logging;

namespace FactorioSharp.Instrumentation.Integration.Jobs.Production;

class UpdatePowerJob : Job
{
    readonly ILogger<UpdatePowerJob> _logger;
    static readonly JsonSerializerOptions JsonSerializerOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

    public UpdatePowerJob(ILogger<UpdatePowerJob> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectAsync(FactorioRconClient client, FactorioData data, FactorioMeasurementOptionsInternal options, CancellationToken cancellationToken)
    {
        ElectricEntity[] entities = await GetElectricEntities(client);
        data.Game.ElectricEntities = entities.ToDictionary(e => e.Name, e => e);

        _logger.LogInformation("Electric entities: {entities}", string.Join(", ", entities.Select(e => $"{e.Name} ({e.Type})")));
    }

    public override async Task OnTickAsync(FactorioRconClient client, FactorioData data, FactorioMeasurementOptionsInternal options, CancellationToken cancellationToken)
    {
        Dictionary<string, Dictionary<uint, Dictionary<string, EntityConsumptionData>>>? consumption = await GetConsumptionData(client);
        if (consumption == null)
        {
            return;
        }

        foreach ((string surface, FactorioSurfaceData surfaceData) in data.Game.Surfaces)
        {
            Dictionary<uint, Dictionary<string, EntityConsumptionData>>? surfaceConsumption = consumption[surface];
            Dictionary<uint, FactorioElectronicNetworkData> networks = new();

            foreach ((uint networkId, Dictionary<string, EntityConsumptionData> entityConsumptions) in surfaceConsumption)
            {
                FactorioFlowData<double> flow = new();
                Dictionary<string, double> buffer = new();
                Dictionary<string, int> entities = new();

                foreach ((string entity, EntityConsumptionData entityConsumption) in entityConsumptions)
                {
                    flow.Inputs[entity] = entityConsumption.Input;
                    flow.Outputs[entity] = entityConsumption.Output;
                    buffer[entity] = entityConsumption.Buffer;
                    entities[entity] = entityConsumption.Count;
                }

                networks[networkId] = new FactorioElectronicNetworkData
                {
                    Flow = flow,
                    Buffer = buffer,
                    Entities = entities
                };
            }

            surfaceData.ElectricNetworks = networks;
        }
    }

    static async Task<ElectricEntity[]> GetElectricEntities(FactorioRconClient client)
    {
        string result = await client.LowLevelClient.ExecuteAsync(
            """
            local function get_entity_data(entity)
                local result = {
                    name = entity.name,
                    max_energy_usage = entity.max_energy_usage,
                    max_energy_production = entity.max_energy_production
                }
                
                if entity.electric_energy_source_prototype then
                    result.min_energy_usage = entity.electric_energy_source_prototype.drain
                    result.buffer_capacity = entity.electric_energy_source_prototype.buffer_capacity
                end
                
                return result
            end

            local result = {}

            for k, v in pairs(game.entity_prototypes) do
                if v.max_energy_usage > 0 or v.max_energy_production > 0 then
                    table.insert(result, get_entity_data(v))
                end
            end

            rcon.print(game.table_to_json(result));
            """
        );

        return JsonSerializer.Deserialize<ElectricEntity[]>(result, JsonSerializerOptions) ?? Array.Empty<ElectricEntity>();
    }

    static async Task<Dictionary<string, Dictionary<uint, Dictionary<string, EntityConsumptionData>>>?> GetConsumptionData(FactorioRconClient client)
    {
        string result = await client.LowLevelClient.ExecuteAsync(
            """
            local function get_network_data(pole_entity)
                local result = {}
                local statistics = pole_entity.electric_network_statistics
                
                if statistics then
                    for entity, _ in pairs(statistics.input_counts) do
                        if not result[entity] then
                            result[entity] = {}
                        end
                        
                        result[entity].input = statistics.get_flow_count{ name = entity, input = true, precision_index = defines.flow_precision_index.five_seconds, sample_index = 1 }
                        result[entity].all_time_input = statistics.input_counts[entity]
                    end
                    
                    for entity, _ in pairs(statistics.output_counts) do
                        if not result[entity] then
                            result[entity] = {}
                        end
                        
                        result[entity].output = statistics.get_flow_count{ name = entity, input = false, precision_index = defines.flow_precision_index.five_seconds, sample_index = 1 }
                        result[entity].all_time_output = statistics.input_counts[entity]
                    end
                    
                    for entity, _ in pairs(result) do
                        if (result[entity].all_time_input and result[entity].all_time_output) then
                            result[entity].buffer = result[entity].all_time_input - result[entity].all_time_output
                        end
                    end
                else
                    result.invalid = true;
                end
                
                return result
            end

            local result = {}
            for _, surface in pairs(game.surfaces) do
                local surface_result = {}
            
                for _, entity in pairs(surface.find_entities_filtered{ type = "electric-pole" }) do
                    local network_id = entity.electric_network_id
                    if not surface_result[network_id] then
                        surface_result[network_id] = get_network_data(entity)
                    end
                end;
                
                local entities = {}
                local seen = {}
                
                for _, network_result in pairs(surface_result) do
                    for entity, _ in pairs(network_result) do
                        if not seen[entity] then
                            table.insert(entities, entity)
                            seen[entity] = true
                        end
                    end
                end
                
                for _, entity in pairs(surface.find_entities_filtered{ name = entities }) do
                    if surface_result[entity.electric_network_id][entity.name].count then
                        surface_result[entity.electric_network_id][entity.name].count = surface_result[entity.electric_network_id][entity.name].count + 1
                    else
                        surface_result[entity.electric_network_id][entity.name].count = 1
                    end
                end
            
                result[surface.name] = surface_result
            end

            rcon.print(game.table_to_json(result))
            """
        );

        return JsonSerializer.Deserialize<Dictionary<string, Dictionary<uint, Dictionary<string, EntityConsumptionData>>>>(result, JsonSerializerOptions);
    }

    class EntityConsumptionData
    {
        public double Input { get; set; }
        public double Output { get; set; }
        public double Buffer { get; set; }
        public int Count { get; set; }
    }
}
