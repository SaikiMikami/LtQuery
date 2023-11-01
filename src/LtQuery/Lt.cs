using LtQuery.Fluents;

namespace LtQuery;

public static class Lt
{
    public static IQueryFluent<TEntity> Query<TEntity>() where TEntity : class => new QueryFluent<TEntity>();
    public static TProperty Arg<TProperty>(string name) => default!;
}
