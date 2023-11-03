namespace LtQuery.Elements;

public sealed class OrderBy : AbstractImmutable, IEquatable<OrderBy>
{
    public PropertyValue Property { get; }
    public OrderByType Type { get; }
    public OrderBy(PropertyValue property, OrderByType type)
    {
        Property = property;
        Type = type;
    }

    protected override int CreateHashCode()
    {
        var code = 0;
        AddHashCode(ref code, Property);
        AddHashCode(ref code, Type);
        return code;
    }

    public override bool Equals(object? obj) => Equals(obj as OrderBy);
    public bool Equals(OrderBy? other)
    {
        if (other == null)
            return false;

        if (!Property.Equals(other.Property))
            return false;
        if (Type  != other.Type)
            return false;
        return true;
    }
}
