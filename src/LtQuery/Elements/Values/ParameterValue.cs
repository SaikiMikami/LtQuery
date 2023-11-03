namespace LtQuery.Elements.Values;

public sealed class ParameterValue : AbstractImmutable, IValue, IEquatable<ParameterValue>
{
    public string Name { get; }
    public Type Type { get; }
    public ParameterValue(string name, Type type)
    {
        Name = name;
        Type = type;
    }

    protected override int CreateHashCode() => Name.GetHashCode();

    public override bool Equals(object? obj) => Equals(obj as ParameterValue);
    public bool Equals(ParameterValue? other)
    {
        if (other == null)
            return false;

        if (!Name.Equals(other.Name))
            return false;
        if (!Type.Equals(other.Type))
            return false;
        return true;
    }
}
