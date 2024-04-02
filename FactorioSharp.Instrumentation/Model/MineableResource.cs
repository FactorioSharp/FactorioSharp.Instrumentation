namespace FactorioSharp.Instrumentation.Model;

class MineableResource
{
    public MineableResource(string name, string? category)
    {
        Name = name;
        Category = category;
    }

    public string Name { get; set; }
    public string? Category { get; set; }
}
