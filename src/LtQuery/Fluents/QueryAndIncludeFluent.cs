using LtQuery.Elements;

namespace LtQuery.Fluents;

class QueryAndIncludeFluent<TEntity, TPreviousProperty> : IQueryAndIncludeFluent<TEntity, TPreviousProperty> where TEntity : class where TPreviousProperty : class?
{
    readonly QueryFluent<TEntity> _inner;
    readonly List<IncludeData> _current;
    public QueryAndIncludeFluent(QueryFluent<TEntity> inner, List<IncludeData> current)
    {
        _inner = inner;
        _current = current;
    }

    public IQueryFluent<TEntity> Where(IBoolValue value) => _inner.Where(value);
    public IQueryFluent<TEntity> Skip(int count) => _inner.Skip(count);
    public IQueryFluent<TEntity> Skip(string parameterName) => _inner.Skip(parameterName);
    public IQueryFluent<TEntity> Take(int count) => _inner.Take(count);
    public IQueryFluent<TEntity> Take(string parameterName) => _inner.Take(parameterName);
    public IQueryAndOrderByFluent<TEntity> OrderBy(IReadOnlyList<string> property) => _inner.OrderBy(property);
    public IQueryAndOrderByFluent<TEntity> OrderByDescending(IReadOnlyList<string> property) => _inner.OrderByDescending(property);
    public IQueryAndIncludeFluent<TEntity, TProperty> Include<TProperty>(IReadOnlyList<string> property) where TProperty : class? => _inner.Include<TProperty>(property);

    public IQueryAndIncludeFluent<TEntity, TProperty> ThenInclude<TProperty>(IReadOnlyList<string> property) where TProperty : class?
    {
        List<IncludeData> current = _current;
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
        return new QueryAndIncludeFluent<TEntity, TProperty>(_inner, current);
    }


    public Query<TEntity> ToImmutable() => _inner.ToImmutable();
}
