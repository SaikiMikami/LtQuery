using System.Reflection;

namespace LtQuery.Metadata;

public class ForeignKeyMeta : PropertyMeta
{
    public EntityMeta DestEntity { get; private set; } = default!;
    public NavigationMeta Navigation { get; private set; } = default!;
    public NavigationMeta DestNavigation { get; private set; } = default!;
    public ForeignKeyMeta(EntityMeta parent, PropertyInfo info, string name, bool isKey = false) : base(parent, info, name, isKey) { }

    internal void Init(EntityMeta destEntity, NavigationMeta navigation, NavigationMeta destNavigation)
    {
        DestEntity = destEntity;
        Navigation = navigation;
        DestNavigation = destNavigation;
    }
}
