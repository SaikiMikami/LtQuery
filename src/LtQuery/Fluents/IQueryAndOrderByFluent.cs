namespace LtQuery.Fluents;

public interface IQueryAndOrderByFluent<TEntity> : IQueryFluent<TEntity> where TEntity : class
{
    //IQueryAndOrderByFluent<TEntity> ThenBy(string[] property);
    //IQueryAndOrderByFluent<TEntity> ThenByDescending(string[] property);
}
