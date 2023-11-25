using LtQuery.Elements;

namespace LtQuery.Fluents;

public interface IQueryFluent<TEntity> where TEntity : class
{
    IQueryFluent<TEntity> Where(IBoolValue value);
    IQueryFluent<TEntity> Skip(int count);
    IQueryFluent<TEntity> Skip(string parameterName);
    IQueryFluent<TEntity> Take(int count);
    IQueryFluent<TEntity> Take(string parameterName);
    IQueryAndOrderByFluent<TEntity> OrderBy(IReadOnlyList<string> property);
    IQueryAndOrderByFluent<TEntity> OrderByDescending(IReadOnlyList<string> property);
    IQueryAndIncludeFluent<TEntity, TProperty> Include<TProperty>(IReadOnlyList<string> property) where TProperty : class?;

    /// <summary>
    /// convert to Query<TEntity>
    /// </summary>
    /// <returns></returns>
    Query<TEntity> ToImmutable();
}
