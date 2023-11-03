namespace LtQuery;

/// <summary>
/// LtQuery Connection
/// </summary>
public interface ILtConnection : IDisposable
{
    int Count<TEntity>(Query<TEntity> query) where TEntity : class;
    int Count<TEntity, TParameter>(Query<TEntity> query, TParameter values) where TEntity : class;

    /// <summary>
    /// Get entities.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="query"></param>
    /// <returns></returns>
    IReadOnlyList<TEntity> Select<TEntity>(Query<TEntity> query) where TEntity : class;

    /// <summary>
    /// Get entities with parameters.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="query"></param>
    /// <returns></returns>
    IReadOnlyList<TEntity> Select<TEntity, TParameter>(Query<TEntity> query, TParameter values) where TEntity : class;

    TEntity Single<TEntity>(Query<TEntity> query) where TEntity : class;
    TEntity Single<TEntity, TParameter>(Query<TEntity> query, TParameter values) where TEntity : class;

    TEntity First<TEntity>(Query<TEntity> query) where TEntity : class;
    TEntity First<TEntity, TParameter>(Query<TEntity> query, TParameter values) where TEntity : class;
}
