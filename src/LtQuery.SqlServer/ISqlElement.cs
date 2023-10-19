using System.Text;

namespace LtQuery.SqlServer;

interface ISqlElement
{
    StringBuilder Append(StringBuilder strb);
}
