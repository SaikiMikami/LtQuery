using LtQuery.Elements;

namespace LtQuery.Relational.Nodes;

class IncludeData
{
    public string PropertyName { get; set; }
    public List<IncludeData> Includes { get; set; } = new();
    public IncludeData(string propertyName)
    {
        PropertyName = propertyName;
    }
    public IncludeData(Include src)
    {
        PropertyName = src.PropertyName;
        foreach (var include in src.Includes)
        {
            Includes.Add(new(include));
        }
    }
}
