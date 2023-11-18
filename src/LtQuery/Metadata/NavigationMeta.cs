using System.Reflection;

namespace LtQuery.Metadata;

public class NavigationMeta
{
    public EntityMeta Parent { get; }
    public Type Type { get; }
    public string Name { get; }
    public ForeignKeyMeta ForeignKey { get; }
    public NavigationType NavigationType { get; }
    public NavigationMeta Dest { get; private set; } = default!;
    public NavigationMeta(EntityMeta parent, Type type, string name, ForeignKeyMeta foreignKey, NavigationType navigationType)
    {
        Parent = parent;
        Type = type;
        Name = name;
        ForeignKey = foreignKey;
        NavigationType = navigationType;
    }

    internal void Init(NavigationMeta dest)
    {
        Dest = dest;
    }

    public PropertyInfo PropertyInfo => Parent.Type.GetProperty(Name)!;


    public bool IsSplited => Dest.NavigationType == NavigationType.Multi;
}
