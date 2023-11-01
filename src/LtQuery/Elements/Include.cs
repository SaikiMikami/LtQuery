namespace LtQuery.Elements;

public class Include : AbstractImmutable
{
    public string PropertyName { get; }
    public ImmutableArray<Include> Includes { get; }
    public Include(string propertyName, ImmutableArray<Include> includes)
    {
        PropertyName = propertyName;
        Includes = includes;
    }


    protected override int CreateHashCode()
    {
        var code = 0;
        AddHashCode(ref code, PropertyName);
        AddHashCode(ref code, Includes);
        return code;
    }
}
