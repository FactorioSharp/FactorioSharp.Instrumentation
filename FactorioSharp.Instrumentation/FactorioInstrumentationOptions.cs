namespace FactorioSharp.Instrumentation;

public class FactorioInstrumentationOptions
{
    bool _allForces;
    readonly HashSet<string> _forces = new();

    bool _allItems;
    readonly HashSet<string> _items = new();

    bool _allFluids;
    readonly HashSet<string> _fluids = new();

    public FactorioInstrumentationOptions MeasureAllForces()
    {
        _allItems = true;
        return this;
    }

    public FactorioInstrumentationOptions MeasureForces(params string[] forces)
    {
        foreach (string force in forces)
        {
            _forces.Add(force);
        }

        return this;
    }

    public FactorioInstrumentationOptions MeasureAllItems()
    {
        _allItems = true;
        return this;
    }

    public FactorioInstrumentationOptions MeasureItems(params string[] items)
    {
        foreach (string item in items)
        {
            _items.Add(item);
        }

        return this;
    }

    public FactorioInstrumentationOptions MeasureAllFluids()
    {
        _allFluids = true;
        return this;
    }

    public FactorioInstrumentationOptions MeasureFluids(params string[] fluids)
    {
        foreach (string fluid in fluids)
        {
            _fluids.Add(fluid);
        }

        return this;
    }

    #region Internal read access

    internal IReadOnlyCollection<string> Forces => _forces;
    internal IReadOnlyCollection<string> Items => _items;
    internal IReadOnlyCollection<string> Fluids => _fluids;

    #endregion
}
