using LtQuery.Elements;
using LtQuery.Elements.Values;
using LtQuery.Elements.Values.Operators;

namespace LtQuery.Fluents;

public class QueryFluent<TEntity> : IQueryAndOrderByFluent<TEntity>, IQueryAndIncludeFluent<TEntity> where TEntity : class
{
    IBoolValue? _condition;
    List<IncludeData> _includes = new();
    List<OrderBy> _orderBys = new();
    IValue? _skipCount;
    IValue? _takeCount;
    //IncludeData? currentInclude;

    public IQueryFluent<TEntity> Where(IBoolValue value)
    {
        if (_condition == null)
            _condition = value;
        else
            _condition = new AndAlsoOperator(_condition, value);
        return this;
    }
    public IQueryFluent<TEntity> Skip(int count)
    {
        _skipCount = new ConstantValue($"{count}");
        return this;
    }
    public IQueryFluent<TEntity> Skip(string parameterName)
    {
        _skipCount = new ParameterValue(parameterName, typeof(int));
        return this;
    }
    public IQueryFluent<TEntity> Take(int count)
    {
        _takeCount = new ConstantValue($"{count}");
        return this;
    }
    public IQueryFluent<TEntity> Take(string parameterName)
    {
        _takeCount = new ParameterValue(parameterName, typeof(int));
        return this;
    }
    public IQueryAndOrderByFluent<TEntity> OrderBy(IReadOnlyList<string> property)
    {
        _orderBys = new List<OrderBy>()
        {
            new OrderBy(convertToProperty(property), OrderByType.Asc)
        };
        return this;
    }

    public IQueryAndOrderByFluent<TEntity> OrderByDescending(IReadOnlyList<string> property)
    {
        _orderBys = new List<OrderBy>()
        {
            new OrderBy(convertToProperty(property), OrderByType.Desc)
        };
        return this;
    }

    //public IQueryAndOrderByFluent<TEntity> ThenBy(string[] property)
    //{
    //    _orderBys.Add(new (convertToProperty( property), OrderByType.Asc));
    //    return this;
    //}

    //public IQueryAndOrderByFluent<TEntity> ThenByDescending(string[] property)
    //{
    //    _orderBys.Add(new(convertToProperty(property), OrderByType.Desc));
    //    return this;
    //}

    public IQueryAndIncludeFluent<TEntity> Include(IReadOnlyList<string> property)
    {
        List<IncludeData> current = _includes;
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
        return this;
    }
    //public IQueryAndIncludeFluent<TEntity> ThenInclude(string[] property)
    //{
    //    var include = new IncludeData(convertToProperty(property));
    //    currentInclude.Includes.Add(include);
    //    currentInclude = include;
    //    return this;
    //}


    public Query<TEntity> ToImmutable()
        => new(_condition, new(_includes.Select(_ => _.ToImmutable()).ToArray()), new(_orderBys.ToArray()), _skipCount, _takeCount);


    PropertyValue convertToProperty(IReadOnlyList<string> propertyName) => convertToProperty(null, propertyName);
    PropertyValue convertToProperty(PropertyValue? parent, IReadOnlyList<string> propertyName)
    {
        PropertyValue? value = parent;
        for (var i = 0; i < propertyName.Count; i++)
            value = new PropertyValue(value, propertyName[i]);
        return value;
    }
}
