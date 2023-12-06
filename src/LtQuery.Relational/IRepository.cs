namespace LtQuery.Relational;

interface IRepository<TEntity> where TEntity : class
{
    IReadOnlyList<TEntity> Select(LtConnection connection, Query<TEntity> query);
    ValueTask<IReadOnlyList<TEntity>> SelectAsync(LtConnection connection, Query<TEntity> query, CancellationToken cancellationToken = default);

    IReadOnlyList<TEntity> Select<TParameter>(LtConnection connection, Query<TEntity> query, TParameter values);
    ValueTask<IReadOnlyList<TEntity>> SelectAsync<TParameter>(LtConnection connection, Query<TEntity> query, TParameter values, CancellationToken cancellationToken = default);

    TEntity Single(LtConnection connection, Query<TEntity> query);
    ValueTask<TEntity> SingleAsync(LtConnection connection, Query<TEntity> query, CancellationToken cancellationToken = default);

    TEntity Single<TParameter>(LtConnection connection, Query<TEntity> query, TParameter values);
    ValueTask<TEntity> SingleAsync<TParameter>(LtConnection connection, Query<TEntity> query, TParameter values, CancellationToken cancellationToken = default);

    TEntity First(LtConnection connection, Query<TEntity> query);
    ValueTask<TEntity> FirstAsync(LtConnection connection, Query<TEntity> query, CancellationToken cancellationToken = default);

    TEntity First<TParameter>(LtConnection connection, Query<TEntity> query, TParameter values);
    ValueTask<TEntity> FirstAsync<TParameter>(LtConnection connection, Query<TEntity> query, TParameter values, CancellationToken cancellationToken = default);

    int Count(LtConnection connection, Query<TEntity> query);
    ValueTask<int> CountAsync(LtConnection connection, Query<TEntity> query, CancellationToken cancellationToken = default);

    int Count<TParameter>(LtConnection connection, Query<TEntity> query, TParameter values);
    ValueTask<int> CountAsync<TParameter>(LtConnection connection, Query<TEntity> query, TParameter values, CancellationToken cancellationToken = default);

    void Add(LtConnection connection, Span<TEntity> entities);
    ValueTask AddAsync(LtConnection connection, TEntity[] entities, CancellationToken cancellationToken = default);

    void Update(LtConnection connection, Span<TEntity> entities);
    ValueTask UpdateAsync(LtConnection connection, TEntity[] entities, CancellationToken cancellationToken = default);

    void Remove(LtConnection connection, Span<TEntity> entities);
    ValueTask RemoveAsync(LtConnection connection, TEntity[] entities, CancellationToken cancellationToken = default);
}
