using LtQuery.Elements;
using LtQuery.SqlServer.Values;
using System.Text;

namespace LtQuery.SqlServer;

class OrderByData
{
    public PropertyValueData Property { get; }
    public OrderByType Type { get; }
    public OrderByData(PropertyValueData property, OrderByType type)
    {
        Property = property;
        Type = type;
    }

    public StringBuilder Append(StringBuilder strb)
    {
        Property.Append(strb);
        if (Type == OrderByType.Desc)
            strb.Append(" DESC");
        return strb;
    }
}
