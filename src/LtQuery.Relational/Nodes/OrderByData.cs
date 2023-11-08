using LtQuery.Elements;
using LtQuery.Relational.Nodes.Values;

namespace LtQuery.Relational.Nodes;

public class OrderByData
{
    public PropertyValueData Property { get; }
    public OrderByType Type { get; }
    public OrderByData(PropertyValueData property, OrderByType type)
    {
        Property = property;
        Type = type;
    }
}
