namespace LtQuery.Elements.Values;

#pragma warning disable CS0659
public sealed class ParameterValue : AbstractImmutable, IValue, IEquatable<ParameterValue>
#pragma warning restore CS0659
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
        if (ReferenceEquals(this, other))
            return true;
        if (other == null)
            return false;

        if (Name != other.Name)
            return false;
        if (Type != other.Type)
            return false;
        return true;
    }
}
