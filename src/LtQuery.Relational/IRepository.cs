namespace LtQuery.Relational;

interface IRepository<TEntity> where TEntity : class
{
    int Count(LtConnection connection, Query<TEntity> query);
    int Count<TParameter>(LtConnection connection, Query<TEntity> query, TParameter values);

    IReadOnlyList<TEntity> Select(LtConnection connection, Query<TEntity> query);
    IReadOnlyList<TEntity> Select<TParameter>(LtConnection connection, Query<TEntity> query, TParameter values);

    TEntity Single(LtConnection connection, Query<TEntity> query);
    TEntity Single<TParameter>(LtConnection connection, Query<TEntity> query, TParameter values);

    TEntity First(LtConnection connection, Query<TEntity> query);
    TEntity First<TParameter>(LtConnection connection, Query<TEntity> query, TParameter values);
}
