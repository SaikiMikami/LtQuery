using LtQuery.Metadata;

namespace LtQuery.Relational.Nodes.Values;

public class PropertyValueData : IValueData
{
    public TableNode Table { get; }
    public PropertyMeta Meta { get; }
    public PropertyValueData(TableNode table, PropertyMeta meta)
    {
        Table = table;
        Meta = meta;
    }
}
