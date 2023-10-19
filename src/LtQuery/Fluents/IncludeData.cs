using LtQuery.Elements;

namespace LtQuery.Fluents;

class IncludeData
{
    public string PropertyName { get; set; }
    public List<IncludeData> Includes { get; } = new List<IncludeData>();
    public IncludeData(string propertyName)
    {
        PropertyName = propertyName;
    }

    public Include ToImmutable()
    {
        var includes = new Include[Includes.Count];
        for (var i = 0; i < Includes.Count; i++)
        {
            var include = Includes[i];
            includes[i] = include.ToImmutable();
        }
        return new Include(PropertyName, new(includes));
    }
}
