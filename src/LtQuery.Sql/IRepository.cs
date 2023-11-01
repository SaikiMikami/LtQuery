using System.Data.Common;

namespace LtQuery.Sql;

interface IRepository<TEntity> where TEntity : class
{
    int Count(DbConnection connection, Query<TEntity> query);
    int Count<TParameter>(DbConnection connection, Query<TEntity> query, TParameter values);

    IReadOnlyList<TEntity> Select(DbConnection connection, Query<TEntity> query);
    IReadOnlyList<TEntity> Select<TParameter>(DbConnection connection, Query<TEntity> query, TParameter values);

    TEntity Single(DbConnection connection, Query<TEntity> query);
    TEntity Single<TParameter>(DbConnection connection, Query<TEntity> query, TParameter values);

    TEntity First(DbConnection connection, Query<TEntity> query);
    TEntity First<TParameter>(DbConnection connection, Query<TEntity> query, TParameter values);
}
