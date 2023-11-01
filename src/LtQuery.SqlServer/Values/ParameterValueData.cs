using System.Text;

namespace LtQuery.SqlServer.Values;

class ParameterValueData : IValueData
{
    public string Name { get; }
    public ParameterValueData(string name)
    {
        Name = name;
    }

    public StringBuilder Append(StringBuilder strb) => strb.Append("@").Append(Name);
}
