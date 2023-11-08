using LtQuery.Elements;
using LtQuery.Relational.Nodes;
using LtQuery.Relational.Nodes.Values;
using LtQuery.Relational.Nodes.Values.Operators;
using System.Text;

namespace LtQuery.SqlServer;

static class ValueExtensions
{
    public static StringBuilder AppendTable(this StringBuilder _this, TableNode table)
        => _this.Append('[').Append(table.Meta.Name).Append("] AS t").Append(table.Index);

    public static StringBuilder AppendValue(this StringBuilder _this, IValueData value)
    {
        switch (value)
        {
            case ConstantValueData v:
                return _this.AppendValue(v);
            case ParameterValueData v:
                return _this.AppendValue(v);
            case PropertyValueData v:
                return _this.AppendValue(v);
            case AndAlsoOperatorData v:
                return _this.AppendValue(v);
            case EqualOperatorData v:
                return _this.AppendValue(v);
            case GreaterThanOperatorData v:
                return _this.AppendValue(v);
            case GreaterThanOrEqualOperatorData v:
                return _this.AppendValue(v);
            case LessThanOperatorData v:
                return _this.AppendValue(v);
            case LessThanOrEqualOperatorData v:
                return _this.AppendValue(v);
            case NotEqualOperatorData v:
                return _this.AppendValue(v);
            case OrElseOperatorData v:
                return _this.AppendValue(v);
            default:
                throw new InvalidProgramException($"Type [{value.GetType()}] is unknown");
        }
    }

    public static StringBuilder AppendValue(this StringBuilder _this, ConstantValueData value)
        => _this.Append(value.Value);

    public static StringBuilder AppendValue(this StringBuilder _this, ParameterValueData value)
        => _this.Append('@').Append(value.Name);

    public static StringBuilder AppendValue(this StringBuilder _this, PropertyValueData value)
        => _this.Append('t').Append(value.Table.Index).Append(".[").Append(value.Meta.Name).Append("]");

    public static StringBuilder AppendValue(this StringBuilder _this, AndAlsoOperatorData value)
        => _this.AppendValue(value.Lhs).Append(" AND ").AppendValue(value.Rhs);

    public static StringBuilder AppendValue(this StringBuilder _this, EqualOperatorData value)
        => _this.AppendValue(value.Lhs).Append(" = ").AppendValue(value.Rhs);

    public static StringBuilder AppendValue(this StringBuilder _this, GreaterThanOperatorData value)
        => _this.AppendValue(value.Lhs).Append(" > ").AppendValue(value.Rhs);

    public static StringBuilder AppendValue(this StringBuilder _this, GreaterThanOrEqualOperatorData value)
        => _this.AppendValue(value.Lhs).Append(" >= ").AppendValue(value.Rhs);

    public static StringBuilder AppendValue(this StringBuilder _this, LessThanOperatorData value)
        => _this.AppendValue(value.Lhs).Append(" < ").AppendValue(value.Rhs);

    public static StringBuilder AppendValue(this StringBuilder _this, LessThanOrEqualOperatorData value)
        => _this.AppendValue(value.Lhs).Append(" <= ").AppendValue(value.Rhs);

    public static StringBuilder AppendValue(this StringBuilder _this, NotEqualOperatorData value)
        => _this.AppendValue(value.Lhs).Append(" != ").AppendValue(value.Rhs);

    public static StringBuilder AppendValue(this StringBuilder _this, OrElseOperatorData value)
        => _this.AppendValue(value.Lhs).Append(" OR ").AppendValue(value.Rhs);


    public static StringBuilder AppendOrderBy(this StringBuilder _this, OrderByData orderBy)
    {
        _this.AppendValue(orderBy.Property);
        if (orderBy.Type == OrderByType.Desc)
            _this.Append(" DESC");
        return _this;
    }
}
