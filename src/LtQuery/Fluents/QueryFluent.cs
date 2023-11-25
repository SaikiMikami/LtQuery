using LtQuery.Elements;
using LtQuery.Elements.Values;
using LtQuery.Elements.Values.Operators;

namespace LtQuery.Fluents;

class QueryFluent<TEntity> : IQueryFluent<TEntity> where TEntity : class
{
    public IBoolValue? Condition { get; private set; }
    public List<IncludeData> Includes { get; } = new();
    public List<OrderBy> OrderBys { get; private set; } = new();
    public IValue? SkipCount { get; private set; }
    public IValue? TakeCount { get; private set; }

    public IQueryFluent<TEntity> Where(IBoolValue value)
    {
        if (Condition == null)
            Condition = value;
        else
            Condition = new AndAlsoOperator(Condition, value);
        return this;
    }
    public IQueryFluent<TEntity> Skip(int count)
    {
        SkipCount = new ConstantValue($"{count}");
        return this;
    }
    public IQueryFluent<TEntity> Skip(string parameterName)
    {
        SkipCount = new ParameterValue(parameterName, typeof(int));
        return this;
    }
    public IQueryFluent<TEntity> Take(int count)
    {
        TakeCount = new ConstantValue($"{count}");
        return this;
    }
    public IQueryFluent<TEntity> Take(string parameterName)
    {
        TakeCount = new ParameterValue(parameterName, typeof(int));
        return this;
    }
    public IQueryAndOrderByFluent<TEntity> OrderBy(IReadOnlyList<string> property)
    {
        OrderBys = new List<OrderBy>()
        {
            new OrderBy(convertToProperty(property), OrderByType.Asc)
        };
        return new QueryAndOrderByFluent<TEntity>(this);
    }

    public IQueryAndOrderByFluent<TEntity> OrderByDescending(IReadOnlyList<string> property)
    {
        OrderBys = new List<OrderBy>()
        {
            new OrderBy(convertToProperty(property), OrderByType.Desc)
        };
        return new QueryAndOrderByFluent<TEntity>(this);
    }

    public IQueryAndIncludeFluent<TEntity, TProperty> Include<TProperty>(IReadOnlyList<string> property) where TProperty : class?
    {
        List<IncludeData> current = Includes;
        foreach (var p in property)
        {
            var include = current.SingleOrDefault(_ => _.PropertyName == p);
            if (include == null)
            {
                var newInclude = new IncludeData(p);
                current.Add(newInclude);
                current = newInclude.Includes;
            }
            else
            {
                current = include.Includes;
            }
        }
        return new QueryAndIncludeFluent<TEntity, TProperty>(this, current);
    }


    public Query<TEntity> ToImmutable()
        => new(Condition, new(Includes.Select(_ => _.ToImmutable()).ToArray()), new(OrderBys.ToArray()), SkipCount, TakeCount);


    PropertyValue convertToProperty(IReadOnlyList<string> propertyName) => convertToProperty(null, propertyName);
    PropertyValue convertToProperty(PropertyValue? parent, IReadOnlyList<string> propertyName)
    {
        PropertyValue? value = parent;
        for (var i = 0; i < propertyName.Count; i++)
            value = new PropertyValue(value, propertyName[i]);
        return value ?? throw new ArgumentException("propertyName.Count == 0");
    }
}
