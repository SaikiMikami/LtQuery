namespace LtQuery;

public interface IDbSelector
{
    /// <summary>
    /// Executes a query, returning the data typed as <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="query"></param>
    /// <returns></returns>
    IReadOnlyList<TEntity> Select<TEntity>(Query<TEntity> query) where TEntity : class;

    /// <summary>
    /// Executes a query, returning the data typed as <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="query"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask<IReadOnlyList<TEntity>> SelectAsync<TEntity>(Query<TEntity> query, CancellationToken cancellationToken = default) where TEntity : class;

    /// <summary>
    /// Executes a query, returning the data typed as <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="query"></param>
    /// <returns></returns>
    IReadOnlyList<TEntity> Select<TEntity, TParameter>(Query<TEntity> query, TParameter values) where TEntity : class;

    /// <summary>
    /// Executes a query, returning the data typed as <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="query"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask<IReadOnlyList<TEntity>> SelectAsync<TEntity, TParameter>(Query<TEntity> query, TParameter values, CancellationToken cancellationToken = default) where TEntity : class;

    /// <summary>
    /// Executes a single-row query, returning the data typed as <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="query"></param>
    /// <returns></returns>
    TEntity Single<TEntity>(Query<TEntity> query) where TEntity : class;

    /// <summary>
    /// Executes a single-row query, returning the data typed as <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="query"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask<TEntity> SingleAsync<TEntity>(Query<TEntity> query, CancellationToken cancellationToken = default) where TEntity : class;

    /// <summary>
    /// Executes a single-row query, returning the data typed as <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="query"></param>
    /// <returns></returns>
    TEntity Single<TEntity, TParameter>(Query<TEntity> query, TParameter values) where TEntity : class;

    /// <summary>
    /// Executes a single-row query, returning the data typed as <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="query"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask<TEntity> SingleAsync<TEntity, TParameter>(Query<TEntity> query, TParameter values, CancellationToken cancellationToken = default) where TEntity : class;

    /// <summary>
    /// Executes a single-row query, returning the data typed as <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="query"></param>
    /// <returns></returns>
    TEntity First<TEntity>(Query<TEntity> query) where TEntity : class;

    /// <summary>
    /// Executes a single-row query, returning the data typed as <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="query"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask<TEntity> FirstAsync<TEntity>(Query<TEntity> query, CancellationToken cancellationToken = default) where TEntity : class;

    /// <summary>
    /// Executes a single-row query, returning the data typed as <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="query"></param>
    /// <returns></returns>
    TEntity First<TEntity, TParameter>(Query<TEntity> query, TParameter values) where TEntity : class;

    /// <summary>
    /// Executes a single-row query, returning the data typed as <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="query"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask<TEntity> FirstAsync<TEntity, TParameter>(Query<TEntity> query, TParameter values, CancellationToken cancellationToken = default) where TEntity : class;

    /// <summary>
    /// Executes a COUNT query, returning count as int.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="query"></param>
    /// <returns></returns>
    int Count<TEntity>(Query<TEntity> query) where TEntity : class;

    /// <summary>
    /// Executes a COUNT query, returning count as int.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="query"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask<int> CountAsync<TEntity>(Query<TEntity> query, CancellationToken cancellationToken = default) where TEntity : class;

    /// <summary>
    /// Executes a COUNT query, returning count as int.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="query"></param>
    /// <returns></returns>
    int Count<TEntity, TParameter>(Query<TEntity> query, TParameter values) where TEntity : class;

    /// <summary>
    /// Executes a COUNT query, returning count as int.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TParameter"></typeparam>
    /// <param name="query"></param>
    /// <param name="values"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask<int> CountAsync<TEntity, TParameter>(Query<TEntity> query, TParameter values, CancellationToken cancellationToken = default) where TEntity : class;
}
