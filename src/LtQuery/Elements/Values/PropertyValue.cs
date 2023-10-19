namespace LtQuery.Elements;

public sealed class PropertyValue : AbstractImmutable, IValue
{
    public PropertyValue? Parent { get; }
    public string Name { get; }
    public PropertyValue(string name) : this(null, name) { }
    public PropertyValue(PropertyValue? parent, string name)
    {
        Parent = parent;
        Name = name;
    }

    protected override int CreateHashCode() => Name.GetHashCode();
}
