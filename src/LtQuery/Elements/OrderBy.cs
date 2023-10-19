namespace LtQuery.Elements;

public sealed class OrderBy : AbstractImmutable
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
}
