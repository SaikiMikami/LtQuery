namespace LtQuery.Metadata;

public class ForeignKeyMeta : PropertyMeta
{
    public EntityMeta DestEntity { get; private set; } = default!;
    public NavigationMeta Navigation { get; private set; } = default!;
    public NavigationMeta DestNavigation { get; private set; } = default!;
    public ForeignKeyMeta(EntityMeta parent, Type type, string name, bool isKey = false) : base(parent, type, name, isKey) { }

    internal void Init(EntityMeta destEntity, NavigationMeta navigation, NavigationMeta destNavigation)
    {
        DestEntity = destEntity;
        Navigation = navigation;
        DestNavigation = destNavigation;
    }
}
