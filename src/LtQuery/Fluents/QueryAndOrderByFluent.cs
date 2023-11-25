using LtQuery.Elements;
using System.Linq.Expressions;

namespace LtQuery.Fluents;

class QueryAndOrderByFluent<TEntity> : IQueryAndOrderByFluent<TEntity> where TEntity : class
{
    readonly QueryFluent<TEntity> _inner;
    public QueryAndOrderByFluent(QueryFluent<TEntity> inner)
    {
        _inner = inner;
    }

    public IQueryFluent<TEntity> Where(IBoolValue value) => _inner.Where(value);
    public IQueryFluent<TEntity> Skip(int count) => _inner.Skip(count);
    public IQueryFluent<TEntity> Skip(string parameterName) => _inner.Skip(parameterName);
    public IQueryFluent<TEntity> Take(int count) => _inner.Take(count);
    public IQueryFluent<TEntity> Take(string parameterName) => _inner.Take(parameterName);
    public IQueryAndOrderByFluent<TEntity> OrderBy(IReadOnlyList<string> property) => _inner.OrderBy(property);
    public IQueryAndOrderByFluent<TEntity> OrderByDescending(IReadOnlyList<string> property) => _inner.OrderByDescending(property);
    public IQueryAndIncludeFluent<TEntity, TProperty> Include<TProperty>(IReadOnlyList<string> property) where TProperty : class? => _inner.Include<TProperty>(property);
    public IQueryAndIncludeFluent<TEntity, TProperty> Include<TProperty>(Expression<Func<TEntity, TProperty>> expression) where TProperty : class => _inner.Include(expression);


    public IQueryAndOrderByFluent<TEntity> ThenBy(IReadOnlyList<string> property)
    {
        _inner.OrderBys.Add(new(convertToProperty(property), OrderByType.Asc));
        return this;
    }

    public IQueryAndOrderByFluent<TEntity> ThenByDescending(IReadOnlyList<string> property)
    {
        _inner.OrderBys.Add(new(convertToProperty(property), OrderByType.Desc));
        return this;
    }


    public Query<TEntity> ToImmutable() => _inner.ToImmutable();


    PropertyValue convertToProperty(IReadOnlyList<string> propertyName) => convertToProperty(null, propertyName);
    PropertyValue convertToProperty(PropertyValue? parent, IReadOnlyList<string> propertyName)
    {
        PropertyValue? value = parent;
        for (var i = 0; i < propertyName.Count; i++)
            value = new PropertyValue(value, propertyName[i]);
        return value ?? throw new ArgumentException("propertyName.Count == 0");
    }
}
