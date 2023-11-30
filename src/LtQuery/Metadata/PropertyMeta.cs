using System.Reflection;

namespace LtQuery.Metadata;

public class PropertyMeta
{
    public EntityMeta Parent { get; }
    public PropertyInfo Info { get; }
    public string Name { get; }
    public bool IsKey { get; }
    public PropertyMeta(EntityMeta parent, PropertyInfo info, string name, bool isKey)
    {
        Parent = parent;
        Info = info;
        Name = name;
        IsKey = isKey;
    }
    public Type Type => Info.PropertyType;

    public bool IsNullable => Type.IsNullable() || Info.IsNullableReference();

    public virtual bool IsAutoIncrement => IsKey && Type != typeof(string);
}
