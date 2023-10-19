using LtQuery.Elements;
using LtQuery.Elements.Values;
using LtQuery.Elements.Values.Operators;
using LtQuery.Sql.Generators;
using LtQuery.SqlServer.Values;
using LtQuery.SqlServer.Values.Operators;

namespace LtQuery.SqlServer;

class QueryTree
{
    public TableTree TopTable { get; }
    public IReadOnlyList<IBoolValueData> Conditions { get; private set; }
    public IReadOnlyList<OrderByData> OrderBys { get; }
    public IValueData? Skip { get; }
    public IValueData? Take { get; }
    public IncludeParentType IncludeParentType { get; }
    public QueryTree? Parent { get; }
    public List<QueryTree> Children { get; } = new();
    public QueryTree(QueryNode current, IBoolValue? condition, IValue? skipCount, IValue? takeCount, ImmutableArray<OrderBy> orderBys, ref int tableIndex)
    {
        TopTable = new TableTree(this, null, current, ref tableIndex);
        if (skipCount != null)
            Skip = convert(skipCount);
        if (takeCount != null)
            Take = convert(takeCount);
        IncludeParentType = getIncludeParentType();

        OrderBys = createOrderBys(orderBys);

        foreach (var child in current.Children)
        {
            if (child.Navigation!.IsSplited)
            {
                var childQuery = new QueryTree(this, child, ref tableIndex);
                Children.Add(childQuery);
            }
        }

        // HACK: Not here
        if (condition != null)
        {
            var conditions = createCondition(condition)!;
            var allConditions = new List<IBoolValueData>();
            split(allConditions, conditions);

            // このクエリに関係あるConditionsを抽出
            Conditions = relatedValueData(allConditions);

            foreach (var child in Children)
            {
                child.Conditions = child.relatedValueData(allConditions);
            }
        }
        else
        {
            Conditions = Array.Empty<IBoolValueData>();
            foreach (var child in Children)
            {
                child.Conditions = Conditions;
            }
        }
    }
    public QueryTree(QueryTree parent, QueryNode node, ref int tableIndex)
    {
        Parent = parent;
        TopTable = new TableTree(this, parent.TopTable, node, ref tableIndex);
        //Conditions = Array.Empty<IBoolValueData>();
        OrderBys = Array.Empty<OrderByData>();
        IncludeParentType = getIncludeParentType();

        foreach (var child in node.Children)
        {
            if (child.Navigation!.IsSplited)
            {
                var childQuery = new QueryTree(this, child, ref tableIndex);
                Children.Add(childQuery);
            }
        }
    }

    IReadOnlyList<OrderByData> createOrderBys(ImmutableArray<OrderBy> orderBys)
    {
        var list = new List<OrderByData>();
        foreach (var orderBy in orderBys)
        {
            var property = convertProperty(orderBy.Property);
            list.Add(new(property, orderBy.Type));
        }
        return list;
    }
    IBoolValueData? createCondition(IBoolValue? condition)
    {
        if (condition == null)
            return null;
        return (IBoolValueData)convert(condition);
    }
    IncludeParentType getIncludeParentType()
    {
        if (Parent == null)
            return IncludeParentType.None;
        if (Parent.Skip != null || Parent.Take != null)
            return IncludeParentType.SubQuery;
        return IncludeParentType.Join;
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
                return new ParameterValueData(v.Name);
            case EqualOperator v:
                return new EqualOperatorData(convert(v.Lhs), convert(v.Rhs));
            case NotEqualOperator v:
                return new NotEqualOperatorData(convert(v.Lhs), convert(v.Rhs));
            case LessThanOperator v:
                return new LessThanOperatorData(convert(v.Lhs), convert(v.Rhs));
            case LessThanOrEqualOperator v:
                return new LessThanOrEqualOperatorData(convert(v.Lhs), convert(v.Rhs));
            case GreaterThanOperator v:
                return new GreaterThanOperatorData(convert(v.Lhs), convert(v.Rhs));
            case GreaterThanOrEqualOperator v:
                return new GreaterThanOrEqualOperatorData(convert(v.Lhs), convert(v.Rhs));
            case AndAlsoOperator v:
                return new AndAlsoOperatorData(convert(v.Lhs), convert(v.Rhs));
            case OrElseOperator v:
                return new OrElseOperatorData(convert(v.Lhs), convert(v.Rhs));
            default:
                throw new InvalidProgramException();
        }
    }
    // Andを平坦にする
    void split(List<IBoolValueData> list, IBoolValueData source)
    {
        if (source is AndAlsoOperatorData)
        {
            var and = (AndAlsoOperatorData)source;
            split(list, (IBoolValueData)and.Lhs);
            split(list, (IBoolValueData)and.Rhs);
        }
        else
            list.Add(source);

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

        var table = TopTable;
        while (queue.Count > 1)
        {
            var v2 = queue.Pop();
            table = table.Children.SingleOrDefault(_ => _.Node.Navigation.Dest.Name == v2.Name) ?? throw new InvalidProgramException($"{table.Node.Type}.[{v2}] not found");
        }
        var name = queue.Pop().Name;
        var p = table.Node.Meta.Properties.SingleOrDefault(_ => _.Name == name) ?? throw new InvalidProgramException($"Property[{name}] not found for [{table.Node.Meta.Name}]Table");
        return new(table, p);
    }

    public IReadOnlyList<IBoolValueData> getAllConditions()
    {
        var list = new List<IBoolValueData>();

        var query = this;
        while (query != null)
        {
            list.AddRange(query.Conditions);
            query = query.Parent;
        }
        return list;
    }


    IReadOnlyList<IBoolValueData> relatedValueData(IReadOnlyList<IBoolValueData> allConditions)
    {
        var list = new List<IBoolValueData>();
        foreach (var boolValue in allConditions)
        {
            var relatedTables = RelatedTables(boolValue);
            if (related(relatedTables, TopTable))
                list.Add(boolValue);
        }
        return list;
    }
    static bool related(IReadOnlyList<TableTree> tables, TableTree table)
    {
        if (tables.Contains(table))
            return true;
        foreach (var child in table.Children)
        {
            if (related(tables, child))
                return true;
        }
        return false;
    }
    public static IReadOnlyList<TableTree> RelatedTables(IValueData condition)
    {
        var set = new HashSet<TableTree>();
        relatedTables(set, condition);
        return set.ToArray();
    }
    static void relatedTables(HashSet<TableTree> set, IValueData condition)
    {
        switch (condition)
        {
            case IBinaryOperatorData v0:
                relatedTables(set, v0.Lhs);
                relatedTables(set, v0.Rhs);
                break;
            case PropertyValueData v1:
                set.Add(v1.Table);
                break;
        }
    }
}
