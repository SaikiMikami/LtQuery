namespace LtQuery.Elements.Values;

public sealed class ParameterValue : AbstractImmutable, IValue
{
    public string Name { get; }
    public Type Type { get; }
    public ParameterValue(string name, Type type)
    {
        Name = name;
        Type = type;
    }

    protected override int CreateHashCode() => Name.GetHashCode();
}
