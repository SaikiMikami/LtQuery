namespace LtQuery.Elements;

public sealed class PropertyValue : AbstractImmutable, IValue, IEquatable<PropertyValue>
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

    public override bool Equals(object? obj) => Equals(obj as PropertyValue);
    public bool Equals(PropertyValue? other)
    {
        if (other == null)
            return false;

        if (!Equals(Parent, other.Parent))
            return false;
        if (!Name.Equals(other.Name))
            return false;
        return true;
    }
}
