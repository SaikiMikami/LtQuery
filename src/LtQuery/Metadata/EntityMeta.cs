namespace LtQuery.Metadata;

public class EntityMeta
{
    public Type Type { get; }
    public string Name { get; }
    public List<PropertyMeta> Properties { get; } = new();
    public List<NavigationMeta> Navigations { get; } = new();

    public EntityMeta(Type type, string? name = default)
    {
        Type = type;
        Name = name ?? Type.Name;
    }

    PropertyMeta? _key;
    public PropertyMeta Key
    {
        get
        {
            if (_key == null)
                _key = Properties.First();
            return _key;
        }
    }
}
