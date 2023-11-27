using LtQuery.Elements;
using LtQuery.Metadata;
using LtQuery.Relational.Generators;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace LtQuery.Relational;

class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    readonly EntityMetaService _metaService;
    readonly ISqlBuilder _sqlBuilder;
    public Repository(EntityMetaService metaService, ISqlBuilder sqlBuilder)
    {
        _metaService = metaService;
        _sqlBuilder = sqlBuilder;
    }

    class Cache2
    {
        public Func<DbCommand, IReadOnlyList<TEntity>> Read { get; set; }
        public string Sql { get; }
        public Cache2(Func<DbCommand, IReadOnlyList<TEntity>> read, string sql)
        {
            Read = read;
            Sql = sql;
        }
    }
    class Cache2<TParameter>
    {
        public Func<DbCommand, TParameter, IReadOnlyList<TEntity>> Read { get; set; }
        public string Sql { get; }
        public Cache2(Func<DbCommand, TParameter, IReadOnlyList<TEntity>> read, string sql)
        {
            Read = read;
            Sql = sql;
        }
    }

    interface IReaderCache { }
    class Cache : IReaderCache
    {
        public Cache2? Select { get; set; }
        public Cache2? First { get; set; }
        public Cache2? Single { get; set; }
    }
    class Cache<TParameter> : IReaderCache
    {
        public Cache2<TParameter>? Select { get; set; }
        public Cache2<TParameter>? First { get; set; }
        public Cache2<TParameter>? Single { get; set; }
    }

    readonly ConditionalWeakTable<Query<TEntity>, IReaderCache> _caches = new();
    readonly ReaderWriterLockSlim _cachesLocker = new();

    Cache getReaderCache(Query<TEntity> query)
    {
        _cachesLocker.EnterUpgradeableReadLock();
        try
        {
            if (!_caches.TryGetValue(query, out var cache))
            {
                _cachesLocker.EnterWriteLock();
                try
                {
                    cache = new Cache();
                    _caches.Add(query, cache);
                }
                finally
                {
                    _cachesLocker.ExitWriteLock();
                }
            }
            return (Cache)cache;
        }
        finally
        {
            _cachesLocker.ExitUpgradeableReadLock();
        }
    }
    Cache<TParameter> getReaderCache<TParameter>(Query<TEntity> query)
    {
        _cachesLocker.EnterUpgradeableReadLock();
        try
        {
            if (!_caches.TryGetValue(query, out var cache))
            {
                _cachesLocker.EnterWriteLock();
                try
                {
                    cache = new Cache<TParameter>();
                    _caches.Add(query, cache);
                }
                finally
                {
                    _cachesLocker.ExitWriteLock();
                }
            }
            return (Cache<TParameter>)cache;
        }
        finally
        {
            _cachesLocker.ExitUpgradeableReadLock();
        }
    }

    class UpdateCache2
    {
        public Action<DbCommand, IEnumerable<TEntity>> Execute { get; set; }
        public string Sql { get; }
        public UpdateCache2(Action<DbCommand, IEnumerable<TEntity>> execute, string sql)
        {
            Execute = execute;
            Sql = sql;
        }
    }
    static class UpdateCache
    {
        public static UpdateCache2? Add { get; set; }
        public static UpdateCache2? Update { get; set; }
        public static UpdateCache2? Remove { get; set; }
    }


    public IReadOnlyList<TEntity> Select(LtConnection connection, Query<TEntity> query)
    {
        var cache = getReaderCache(query);

        var cache2 = cache.Select;
        if (cache2 == null)
        {
            var read = new ReadGenerator<TEntity>(_metaService).CreateReadSelectFunc(query);
            var sql = _sqlBuilder.CreateSelectSql(query);
            cache2 = new(read, sql);
            cache.Select = cache2;
        }

        var command = connection.GetSelectCommand(query, cache2.Sql);

        return cache2.Read(command);
    }

    public IReadOnlyList<TEntity> Select<TParameter>(LtConnection connection, Query<TEntity> query, TParameter values)
    {
        var cache = getReaderCache<TParameter>(query);

        var cache2 = cache.Select;
        if (cache2 == null)
        {
            var read = new ReadGenerator<TEntity>(_metaService).CreateReadSelectFunc<TParameter>(query);
            var sql = _sqlBuilder.CreateSelectSql(query);
            cache2 = new(read, sql);

            cache.Select = cache2;
        }

        var command = connection.GetSelectCommand(query, cache2.Sql);

        return cache2.Read(command, values);
    }

    public TEntity Single(LtConnection connection, Query<TEntity> query)
    {
        var cache = getReaderCache(query);

        var cache2 = cache.Single;
        if (cache2 == null)
        {
            var signleQuery = new Query<TEntity>(query.Condition, query.Includes, query.OrderBys, query.SkipCount, new ConstantValue("2"));
            var read = new ReadGenerator<TEntity>(_metaService).CreateReadSelectFunc(signleQuery);
            var sql = _sqlBuilder.CreateSelectSql(signleQuery);
            cache2 = new(read, sql);

            cache.Single = cache2;
        }

        var command = connection.GetSingleCommand(query, cache2.Sql);

        return cache2.Read(command).Single();
    }

    public TEntity Single<TParameter>(LtConnection connection, Query<TEntity> query, TParameter values)
    {
        var cache = getReaderCache<TParameter>(query);

        var cache2 = cache.Single;
        if (cache2 == null)
        {
            var signleQuery = new Query<TEntity>(query.Condition, query.Includes, query.OrderBys, query.SkipCount, new ConstantValue("2"));
            var read = new ReadGenerator<TEntity>(_metaService).CreateReadSelectFunc<TParameter>(signleQuery);
            var sql = _sqlBuilder.CreateSelectSql(signleQuery);
            cache2 = new(read, sql);

            cache.Single = cache2;
        }

        var command = connection.GetSingleCommand(query, cache2.Sql);
        if (connection.CurrentTransaction != null)
            command.Transaction = connection.CurrentTransaction.Inner;

        return cache2.Read(command, values).Single();
    }

    public TEntity First(LtConnection connection, Query<TEntity> query)
    {
        var cache = getReaderCache(query);

        var cache2 = cache.First;
        if (cache2 == null)
        {
            var firstQuery = new Query<TEntity>(query.Condition, query.Includes, query.OrderBys, query.SkipCount, new ConstantValue("1"));
            var read = new ReadGenerator<TEntity>(_metaService).CreateReadSelectFunc(firstQuery);
            var sql = _sqlBuilder.CreateSelectSql(firstQuery);
            cache2 = new(read, sql);

            cache.First = cache2;
        }

        var command = connection.GetFirstCommand(query, cache2.Sql);
        if (connection.CurrentTransaction != null)
            command.Transaction = connection.CurrentTransaction.Inner;

        return cache2.Read(command).First();
    }

    public TEntity First<TParameter>(LtConnection connection, Query<TEntity> query, TParameter values)
    {
        var cache = getReaderCache<TParameter>(query);

        var cache2 = cache.First;
        if (cache2 == null)
        {
            var firstQuery = new Query<TEntity>(query.Condition, query.Includes, query.OrderBys, query.SkipCount, new ConstantValue("1"));
            var read = new ReadGenerator<TEntity>(_metaService).CreateReadSelectFunc<TParameter>(firstQuery);
            var sql = _sqlBuilder.CreateSelectSql(firstQuery);
            cache2 = new(read, sql);

            cache.First = cache2;
        }

        var command = connection.GetFirstCommand(query, cache2.Sql);
        if (connection.CurrentTransaction != null)
            command.Transaction = connection.CurrentTransaction.Inner;

        return cache2.Read(command, values).First();
    }

    public int Count(LtConnection connection, Query<TEntity> query)
    {
        throw new NotImplementedException();
    }

    public int Count<TParameter>(LtConnection connection, Query<TEntity> query, TParameter values)
    {
        throw new NotImplementedException();
    }

    public void Add(LtConnection connection, IEnumerable<TEntity> entities)
    {
        var cache = UpdateCache.Add;
        if (cache == null)
        {
            var execute = new ReadGenerator<TEntity>(_metaService).CreateExecuteUpdateFunc(DbMethod.Add);
            var sql = _sqlBuilder.CreateAddSql<TEntity>();
            cache = new(execute, sql);

            UpdateCache.Add = cache;
        }

        var command = connection.GetAddCommand<TEntity>(cache.Sql);
        if (connection.CurrentTransaction != null)
            command.Transaction = connection.CurrentTransaction.Inner;

        cache.Execute(command, entities);
    }

    public void Update(LtConnection connection, IEnumerable<TEntity> entities)
    {
        var cache = UpdateCache.Update;
        if (cache == null)
        {
            var execute = new ReadGenerator<TEntity>(_metaService).CreateExecuteUpdateFunc(DbMethod.Update);
            var sql = _sqlBuilder.CreateUpdatedSql<TEntity>();
            cache = new(execute, sql);

            UpdateCache.Update = cache;
        }

        var command = connection.GetUpdateCommand<TEntity>(cache.Sql);
        if (connection.CurrentTransaction != null)
            command.Transaction = connection.CurrentTransaction.Inner;

        cache.Execute(command, entities);
    }

    public void Remove(LtConnection connection, IEnumerable<TEntity> entities)
    {
        var cache = UpdateCache.Remove;
        if (cache == null)
        {
            var execute = new ReadGenerator<TEntity>(_metaService).CreateExecuteUpdateFunc(DbMethod.Remove);
            var sql = _sqlBuilder.CreateRemoveSql<TEntity>();
            cache = new(execute, sql);

            UpdateCache.Remove = cache;
        }

        var command = connection.GetRemoveCommand<TEntity>(cache.Sql);
        if (connection.CurrentTransaction != null)
            command.Transaction = connection.CurrentTransaction.Inner;

        cache.Execute(command, entities);
    }
}
