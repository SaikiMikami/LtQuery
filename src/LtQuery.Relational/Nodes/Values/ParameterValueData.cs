namespace LtQuery.Relational.Nodes.Values;

public class ParameterValueData : IValueData
{
    public string Name { get; }
    public Type Type { get; }
    public ParameterValueData(string name, Type type)
    {
        Name = name;
        Type = type;
    }
}
