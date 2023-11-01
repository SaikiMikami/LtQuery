using System.Reflection;

namespace LtQuery.Metadata;

public class NavigationMeta
{
    public EntityMeta Parent { get; }
    public Type Type { get; }
    public string Name { get; }
    public ForeignKeyMeta ForeignKey { get; }
    public bool IsUnique { get; }
    public NavigationType NavigationType { get; }
    public NavigationMeta Dest { get; private set; } = default!;
    public NavigationMeta(EntityMeta parent, Type type, string name, ForeignKeyMeta foreignKey, bool isUnique, NavigationType navigationType)
    {
        Parent = parent;
        Type = type;
        Name = name;
        ForeignKey = foreignKey;
        IsUnique = isUnique;
        NavigationType = navigationType;
    }

    internal void Init(NavigationMeta dest)
    {
        Dest = dest;
    }

    public PropertyInfo PropertyInfo => Parent.Type.GetProperty(Name)!;


    public bool IsSplited => Dest.NavigationType == NavigationType.Multi;
}
