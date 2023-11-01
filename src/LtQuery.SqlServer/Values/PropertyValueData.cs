using LtQuery.Metadata;
using System.Text;

namespace LtQuery.SqlServer.Values;

class PropertyValueData : IValueData
{
    public TableTree Table { get; }
    public PropertyMeta Meta { get; }
    public PropertyValueData(TableTree table, PropertyMeta meta)
    {
        Table = table;
        Meta = meta;
    }

    public StringBuilder Append(StringBuilder strb)
        => strb.Append("t").Append(Table.Index).Append(".[").Append(Meta.Name).Append(']');
}
