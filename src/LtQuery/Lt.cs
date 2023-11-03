using LtQuery.Fluents;

namespace LtQuery;

/// <summary>
/// Query<> generation with LINQ
/// </summary>
public static class Lt
{
    /// <summary>
    /// Query<> generation with LINQ
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <returns></returns>
    public static IQueryFluent<TEntity> Query<TEntity>() where TEntity : class => new QueryFluent<TEntity>();

    /// <summary>
    /// Parameter
    /// </summary>
    /// <typeparam name="TProperty">Parameter type</typeparam>
    /// <param name="name">Parameter name</param>
    /// <returns></returns>
    public static TProperty Arg<TProperty>(string name) => default!;
}
