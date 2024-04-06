namespace FactorioSharp.Instrumentation.Model;

class ElectricEntity
{
    public string Name { get; set; }

    public double BufferCapacity { get; set; }
    public double MaxEnergyUsage { get; set; }
    public double MaxEnergyProduction { get; set; }

    public ElectricEntityType Type {
        get {
            ElectricEntityType result = ElectricEntityType.None;

            if (MaxEnergyUsage > 0)
            {
                result |= ElectricEntityType.Sink;
            }

            if (MaxEnergyProduction > 0)
            {
                result |= ElectricEntityType.Source;
            }

            if (BufferCapacity > 0 && MaxEnergyProduction > 0)
            {
                result |= ElectricEntityType.Accumulator;
            }

            return result;
        }
    }
}
