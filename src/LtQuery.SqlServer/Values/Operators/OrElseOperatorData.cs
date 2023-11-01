using System.Text;

namespace LtQuery.SqlServer.Values.Operators;

class OrElseOperatorData : IBinaryOperatorData
{
    public IValueData Lhs { get; }
    public IValueData Rhs { get; }
    public OrElseOperatorData(IValueData lhs, IValueData rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }

    public StringBuilder Append(StringBuilder strb)
    {
        Lhs.Append(strb).Append(" OR ");
        Rhs.Append(strb);
        return strb;
    }
}
