using LtQuery.Elements;
using LtQuery.Elements.Values;
using LtQuery.Metadata;

namespace LtQuery.Relational.Nodes;

public class TableNode
{
    public EntityMeta Meta { get; }
    public NavigationMeta? Navigation { get; }
    public TableNode? Parent { get; }
    public List<TableNode> Children { get; } = new();
    public int Index { get; }
    public TableNode(EntityMeta meta, IBoolValue? condition, IReadOnlyList<Include> includes, IReadOnlyList<OrderBy> orderBys, IValue? skipCount, IValue? takeCount)
    {
        Meta = meta;
        Navigation = null;
        Parent = null;
        var index = 0;
        Index = index++;

        var includes2 = createIncludeDatas(condition, includes, orderBys, skipCount, takeCount);

        foreach (var include in includes2)
        {
            var nav = Meta.Navigations.Single(_ => _.Name == include.PropertyName);
            Children.Add(new(this, nav.Dest, include, ref index));
        }
    }
    private TableNode(TableNode parent, NavigationMeta navigation, IncludeData include, ref int index)
    {
        Meta = navigation.Parent;
        Parent = parent;
        Navigation = navigation;
        Index = index++;

        if (include != null)
        {
            foreach (var child in include.Includes)
            {
                var nav = Meta.Navigations.Single(_ => _.Name == child.PropertyName);
                Children.Add(new(this, nav.Dest, child, ref index));
            }
        }
    }

    static IReadOnlyList<IncludeData> createIncludeDatas(IBoolValue? condition, IReadOnlyList<Include> includes, IReadOnlyList<OrderBy> orderBys, IValue? skipCount, IValue? takeCount)
    {
        var propertyValues = new List<PropertyValue>();
        if (condition != null)
            addPropertyValues(propertyValues, condition);
        foreach (var orderBy in orderBys)
            addPropertyValues(propertyValues, orderBy.Property);
        if (skipCount != null)
            addPropertyValues(propertyValues, skipCount);
        if (takeCount != null)
            addPropertyValues(propertyValues, takeCount);

        var includeDatas = new List<IncludeData>();
        foreach (var include in includes)
            includeDatas.Add(new(include));

        foreach (var propertyValue in propertyValues)
            addInclude(includeDatas, propertyValue);
        return includeDatas;
    }

    static void addPropertyValues(List<PropertyValue> list, IValue value)
    {
        switch (value)
        {
            case PropertyValue property:
                list.Add(property);
                return;
            case IBinaryOperator binary:
                addPropertyValues(list, binary.Lhs);
                addPropertyValues(list, binary.Rhs);
                return;
        }
    }
    static void addInclude(List<IncludeData> includes, PropertyValue propertyValue)
    {
        var stack = new Stack<string>();
        PropertyValue? v = propertyValue;
        while (v != null)
        {
            stack.Push(v.Name);
            v = v.Parent;
        }

        while (stack.Count > 1)
        {
            var propertyName = stack.Pop();
            var include = includes.SingleOrDefault(_ => _.PropertyName == propertyName);
            if (include == null)
            {
                include = new(propertyName);
                includes.Add(include);
            }
            includes = include.Includes;
        }
    }

    public Type Type => Meta.Type;
    public PropertyMeta Key => Meta.Key;
    public int PropertyCount => Meta.Properties.Count;

    public bool HasSubQuery() => Children.Any(_ => _.Navigation!.IsSplited);
}
