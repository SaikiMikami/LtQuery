using LtQuery.Metadata;
using System.Data.Common;

namespace LtQuery.Sql;

class LtConnection : ILtConnection
{
    readonly EntityMetaService _metaService;
    readonly ISqlBuilder _sqlBuilder;
    readonly DbConnection _connection;
    public LtConnection(EntityMetaService metaService, ISqlBuilder sqlBuilder, DbConnection connection)
    {
        _metaService = metaService;
        _sqlBuilder = sqlBuilder;
        _connection = connection;
        _connection.Open();
    }
    public void Dispose()
    {
        _connection.Dispose();
    }

    static class RepositoryCache<TEntity> where TEntity : class
    {
        public static IRepository<TEntity>? Repository = default;
    }
    IRepository<TEntity> getRepository<TEntity>() where TEntity : class
    {
        var repository = RepositoryCache<TEntity>.Repository;
        if (repository == null)
        {
            repository = createRepository<TEntity>();
            RepositoryCache<TEntity>.Repository = repository;
        }
        return repository;
    }
    IRepository<TEntity> createRepository<TEntity>() where TEntity : class
    {
        return new Repository<TEntity>(_metaService, _sqlBuilder);
    }



    public int Count<TEntity>(Query<TEntity> query) where TEntity : class => getRepository<TEntity>().Count(_connection, query);

    public int Count<TEntity, TParameter>(Query<TEntity> query, TParameter values) where TEntity : class => getRepository<TEntity>().Count(_connection, query, values);

    public TEntity First<TEntity>(Query<TEntity> query) where TEntity : class => getRepository<TEntity>().First(_connection, query);

    public TEntity First<TEntity, TParameter>(Query<TEntity> query, TParameter values) where TEntity : class => getRepository<TEntity>().First(_connection, query, values);

    public IReadOnlyList<TEntity> Select<TEntity>(Query<TEntity> query) where TEntity : class => getRepository<TEntity>().Select(_connection, query);

    public IReadOnlyList<TEntity> Select<TEntity, TParameter>(Query<TEntity> query, TParameter values) where TEntity : class => getRepository<TEntity>().Select(_connection, query, values);

    public TEntity Single<TEntity>(Query<TEntity> query) where TEntity : class => getRepository<TEntity>().Single(_connection, query);

    public TEntity Single<TEntity, TParameter>(Query<TEntity> query, TParameter values) where TEntity : class => getRepository<TEntity>().Single(_connection, query, values);
}
