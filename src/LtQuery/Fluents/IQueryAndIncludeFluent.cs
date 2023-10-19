namespace LtQuery.Fluents;

public interface IQueryAndIncludeFluent<TEntity> : IQueryFluent<TEntity> where TEntity : class
{
    //IQueryAndIncludeFluent<TEntity> ThenInclude(string[] property);
}
