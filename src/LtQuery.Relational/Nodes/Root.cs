using LtQuery.Elements;
using LtQuery.Elements.Values;
using LtQuery.Elements.Values.Operators;
using LtQuery.Metadata;
using LtQuery.Relational.Nodes.Values;
using LtQuery.Relational.Nodes.Values.Operators;

namespace LtQuery.Relational.Nodes;

public class Root
{
    public QueryNode RootQuery { get; }
    public TableNode RootTable { get; }
    public IReadOnlyList<IBoolValueData> Conditions { get; }
    public IReadOnlyList<ParameterValueData> AllParameters { get; }
    public IReadOnlyList<PropertyValueData> AllProperties { get; }
    public IReadOnlyList<OrderByData> OrderBys { get; }
    public IValueData? SkipCount { get; }
    public IValueData? TakeCount { get; }
    private Root(EntityMeta meta, IBoolValue? condition, IReadOnlyList<Include> includes, IReadOnlyList<OrderBy> orderBys, IValue? skipCount, IValue? takeCount)
    {
        RootTable = new TableNode(meta, condition, includes, orderBys, skipCount, takeCount);

        if (condition != null)
        {
            var allConditios = splitCondition(condition);
            Conditions = convert(allConditios);
        }
        else
        {
            Conditions = Array.Empty<IBoolValueData>();
        }

        OrderBys = convert(orderBys);

        if (skipCount != null)
            SkipCount = convert(skipCount);
        else
            SkipCount = null;

        if (takeCount != null)
            TakeCount = convert(takeCount);
        else
            TakeCount = null;

        AllParameters = getParameters();
        AllProperties = getProperties();
        RootQuery = new QueryNode(this, RootTable);
    }

    public static Root Create<TEntity>(EntityMetaService metaService, Query<TEntity> query) where TEntity : class
        => new(metaService.GetEntityMeta<TEntity>(), query.Condition, query.Includes, query.OrderBys, query.SkipCount, query.TakeCount);

    IReadOnlyList<IBoolValue> splitCondition(IBoolValue condition)
    {
        var list = new List<IBoolValue>();
        splitCondition(list, condition);
        return list;
    }
    void splitCondition(List<IBoolValue> list, IBoolValue condition)
    {
        switch (condition)
        {
            case AndAlsoOperator and:
                list.Add((IBoolValue)and.Lhs);
                list.Add((IBoolValue)and.Rhs);
                return;
            default:
                list.Add(condition);
                return;
        }
    }
    IReadOnlyList<IBoolValueData> convert(IReadOnlyList<IBoolValue> conditions)
    {
        var list = new List<IBoolValueData>();
        foreach (var condition in conditions)
        {
            list.Add((IBoolValueData)convert(condition));
        }
        return list;
    }
    IValueData convert(IValue source)
    {
        switch (source)
        {
            case ConstantValue v:
                return new ConstantValueData(v.Value);
            case PropertyValue v:
                return convertProperty(v);
            case ParameterValue v:
                return new ParameterValueData(v.Name, v.Type);
            case IBinaryOperator v:
                var lhs = convert(v.Lhs);
                var rhs = convert(v.Rhs);
                switch (v)
                {
                    case EqualOperator:
                        return new EqualOperatorData(lhs, rhs);
                    case NotEqualOperator:
                        return new NotEqualOperatorData(lhs, rhs);
                    case LessThanOperator:
                        return new LessThanOperatorData(lhs, rhs);
                    case LessThanOrEqualOperator:
                        return new LessThanOrEqualOperatorData(lhs, rhs);
                    case GreaterThanOperator:
                        return new GreaterThanOperatorData(lhs, rhs);
                    case GreaterThanOrEqualOperator:
                        return new GreaterThanOrEqualOperatorData(lhs, rhs);
                    case AndAlsoOperator:
                        return new AndAlsoOperatorData(lhs, rhs);
                    case OrElseOperator:
                        return new OrElseOperatorData(lhs, rhs);
                    default:
                        throw new InvalidProgramException();
                }
            default:
                throw new InvalidProgramException();
        }
    }

    PropertyValueData convertProperty(PropertyValue value)
    {
        var queue = new Stack<PropertyValue>();
        var v = value;
        while (v != null)
        {
            queue.Push(v);
            v = v.Parent;
        }

        var node = RootTable;
        while (queue.Count > 1)
        {
            var v2 = queue.Pop();
            var child = node.Children.SingleOrDefault(_ => _.Navigation.Dest.Name == v2.Name) ?? throw new InvalidProgramException($"[{node.Meta.Type}.{v2.Name}] not found");
            node = child;
        }
        var name = queue.Pop().Name;
        var p = node.Meta.Properties.SingleOrDefault(_ => _.Name == name) ?? throw new InvalidProgramException($"[{node.Meta.Type}.{name}] not found");
        return new(node, p);
    }

    IReadOnlyList<OrderByData> convert(IReadOnlyList<OrderBy> orderBys)
    {
        var list = new List<OrderByData>();
        foreach (var orderBy in orderBys)
        {
            var property = convertProperty(orderBy.Property);
            list.Add(new(property, orderBy.Type));
        }
        return list;
    }

    IReadOnlyList<ParameterValueData> getParameters()
    {
        var parameters = new List<ParameterValueData>();
        foreach (var condition in Conditions)
        {
            buildParameterValues(parameters, condition);
        }
        if (SkipCount != null)
        {
            buildParameterValues(parameters, SkipCount);
        }
        if (TakeCount != null)
        {
            buildParameterValues(parameters, TakeCount);
        }
        return parameters;
    }

    static void buildParameterValues(List<ParameterValueData> list, IValueData src)
    {
        switch (src)
        {
            case ParameterValueData v0:
                list.Add(v0);
                break;
            case IBinaryOperatorData v1:
                buildParameterValues(list, v1.Lhs);
                buildParameterValues(list, v1.Rhs);
                break;
        }
    }

    IReadOnlyList<PropertyValueData> getProperties()
    {
        var list = new List<PropertyValueData>();
        if (Conditions != null)
        {
            foreach (var condition in Conditions)
                getProperties(list, condition);
        }
        if (SkipCount != null)
            getProperties(list, SkipCount);
        if (TakeCount != null)
            getProperties(list, TakeCount);
        return list;
    }

    static void getProperties(List<PropertyValueData> list, IValueData value)
    {
        switch (value)
        {
            case IBinaryOperatorData v:
                getProperties(list, v.Lhs);
                return;
            case PropertyValueData v:
                list.Add(v);
                return;
        }
    }
}
