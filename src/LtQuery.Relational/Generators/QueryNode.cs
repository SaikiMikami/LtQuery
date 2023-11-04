using LtQuery.Elements;
using LtQuery.Elements.Values;
using LtQuery.Metadata;

namespace LtQuery.Relational.Generators;

public class QueryNode
{
    public EntityMeta Meta { get; }
    public NavigationMeta? Navigation { get; }
    public QueryNode? Parent { get; }
    public List<QueryNode> Children { get; } = new();
    public QueryNode(EntityMeta meta, IBoolValue? condition, IReadOnlyList<Include> includes, IReadOnlyList<OrderBy> orderBys, IValue? skipCount, IValue? takeCount)
    {
        Meta = meta;
        Navigation = null;
        Parent = null;

        var includes2 = createIncludeDatas(condition, includes, orderBys, skipCount, takeCount);


        foreach (var include in includes2)
        {
            var nav = meta.Navigations.Single(_ => _.Name == include.PropertyName);
            Children.Add(new(this, nav.Dest, include));
        }
    }
    public QueryNode(QueryNode parent, NavigationMeta navigation, IncludeData? include)
    {
        Meta = navigation.Parent;
        Parent = parent;
        Navigation = navigation;
    }

    public PropertyMeta Key => Meta.Key;
    public Type Type => Meta.Type;
    public int PropertyCount => Meta.Properties.Count;

    public bool HasSubQuery() => Children.Any(_ => _.Navigation!.IsSplited);


    public class IncludeData
    {
        public string PropertyName { get; set; }
        public List<IncludeData> Includes { get; set; } = new();
        public IncludeData(string propertyName)
        {
            PropertyName = propertyName;
        }
        public IncludeData(Include src)
        {
            PropertyName = src.PropertyName;
            foreach (var include in src.Includes)
            {
                Includes.Add(new(include));
            }
        }
    }
    static IReadOnlyList<IncludeData> createIncludeDatas(IBoolValue? condition, IReadOnlyList<Include> includes, IReadOnlyList<OrderBy> orderBys, IValue? skipCount, IValue? takeCount)
    {
        var list = includes.ToList();
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
}
