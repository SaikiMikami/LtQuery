using LtQuery.Elements;

namespace LtQuery.Fluents;

public interface IQueryFluent<TEntity> where TEntity : class
{
    Query<TEntity> ToImmutable();

    IQueryFluent<TEntity> Where(IBoolValue value);
    IQueryFluent<TEntity> Skip(int count);
    IQueryFluent<TEntity> Skip(string parameterName);
    IQueryFluent<TEntity> Take(int count);
    IQueryFluent<TEntity> Take(string parameterName);
    IQueryAndOrderByFluent<TEntity> OrderBy(IReadOnlyList<string> property);
    IQueryAndOrderByFluent<TEntity> OrderByDescending(IReadOnlyList<string> property);
    IQueryAndIncludeFluent<TEntity> Include(IReadOnlyList<string> property);
}
