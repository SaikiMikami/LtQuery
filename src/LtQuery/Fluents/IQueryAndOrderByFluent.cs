namespace LtQuery.Fluents;

public interface IQueryAndOrderByFluent<TEntity> : IQueryFluent<TEntity> where TEntity : class
{
    IQueryAndOrderByFluent<TEntity> ThenBy(IReadOnlyList<string> property);
    IQueryAndOrderByFluent<TEntity> ThenByDescending(IReadOnlyList<string> property);
}
