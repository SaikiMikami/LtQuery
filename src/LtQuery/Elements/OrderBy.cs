namespace LtQuery.Elements;

#pragma warning disable CS0659
public sealed class OrderBy : AbstractImmutable, IEquatable<OrderBy>
#pragma warning restore CS0659
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
        if (ReferenceEquals(this, other))
            return true;
        if (other == null)
            return false;

        if (!Property.Equals(other.Property))
            return false;
        if (Type != other.Type)
            return false;
        return true;
    }
}
