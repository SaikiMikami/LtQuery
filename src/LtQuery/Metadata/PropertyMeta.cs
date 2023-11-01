namespace LtQuery.Metadata;

public class PropertyMeta
{
    public EntityMeta Parent { get; }
    public Type Type { get; }
    public string Name { get; }
    public bool IsKey { get; }
    public PropertyMeta(EntityMeta parent, Type type, string name, bool isKey)
    {
        Parent = parent;
        Type = type;
        Name = name;
        IsKey = isKey;
    }
}
