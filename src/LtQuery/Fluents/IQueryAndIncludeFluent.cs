namespace LtQuery.Fluents;

public interface IQueryAndIncludeFluent<TEntity, out TPreviousProperty> : IQueryFluent<TEntity> where TEntity : class where TPreviousProperty : class?
{
    IQueryAndIncludeFluent<TEntity, TProperty> ThenInclude<TProperty>(IReadOnlyList<string> property) where TProperty : class?;
}
