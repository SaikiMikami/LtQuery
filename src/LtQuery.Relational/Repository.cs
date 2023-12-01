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
    readonly EntityMeta _meta;
    readonly ReadGenerator<TEntity> _generator;
    public Repository(EntityMetaService metaService, ISqlBuilder sqlBuilder)
    {
        _metaService = metaService;
        _sqlBuilder = sqlBuilder;
        _meta = _metaService.GetEntityMeta<TEntity>();
        _generator = new ReadGenerator<TEntity>(_metaService);
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
        public ExecuteUpdate<TEntity> Execute { get; set; }
        public UpdateCache2(ExecuteUpdate<TEntity> execute)
        {
            Execute = execute;
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
            var read = _generator.CreateReadSelectFunc(query);
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
            var read = _generator.CreateReadSelectFunc<TParameter>(query);
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
            var read = _generator.CreateReadSelectFunc(signleQuery);
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
            var read = _generator.CreateReadSelectFunc<TParameter>(signleQuery);
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
            var read = _generator.CreateReadSelectFunc(firstQuery);
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
            var read = _generator.CreateReadSelectFunc<TParameter>(firstQuery);
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

    const int _maxParameterCount = 2099;

    public void Add(LtConnection connection, Span<TEntity> entities)
    {
        var count = entities.Length;

        var cache = UpdateCache.Add;
        if (cache == null)
        {
            var execute = _generator.CreateExecuteUpdateFunc(DbMethod.Add);
            cache = new(execute);

            UpdateCache.Add = cache;
        }

        var maxLength = _maxParameterCount / _meta.Properties.Count;
        var i = 0;
        while (i < count)
        {
            var length = count - i;
            if (length > maxLength)
                length = maxLength;
            var entities2 = entities.Slice(i, length);

            var sql = _sqlBuilder.CreateAddSql<TEntity>(length);
            var command = connection.GetAddCommand<TEntity>(sql, length);
            if (connection.CurrentTransaction != null)
                command.Transaction = connection.CurrentTransaction.Inner;

            cache.Execute(command, entities2);
            i += length;
        }
    }

    public void Update(LtConnection connection, Span<TEntity> entities)
    {
        var count = entities.Length;

        var cache = UpdateCache.Update;
        if (cache == null)
        {
            var execute = _generator.CreateExecuteUpdateFunc(DbMethod.Update);
            cache = new(execute);

            UpdateCache.Update = cache;
        }

        var maxLength = _maxParameterCount / _meta.Properties.Count;
        var i = 0;
        while (i < count)
        {
            var length = count - i;
            if (length > maxLength)
                length = maxLength;
            var entities2 = entities.Slice(i, length);

            var sql = _sqlBuilder.CreateUpdatedSql<TEntity>(length);
            var command = connection.GetUpdateCommand<TEntity>(sql, length);
            if (connection.CurrentTransaction != null)
                command.Transaction = connection.CurrentTransaction.Inner;

            cache.Execute(command, entities2);
            i += length;
        }
    }

    public void Remove(LtConnection connection, Span<TEntity> entities)
    {
        var count = entities.Length;

        var cache = UpdateCache.Remove;
        if (cache == null)
        {
            var execute = _generator.CreateExecuteUpdateFunc(DbMethod.Remove);
            cache = new(execute);

            UpdateCache.Remove = cache;
        }

        var maxLength = _maxParameterCount / _meta.Properties.Count;
        var i = 0;
        while (i < count)
        {
            var length = count - i;
            if (length > maxLength)
                length = maxLength;
            var entities2 = entities.Slice(i, length);

            var sql = _sqlBuilder.CreateRemoveSql<TEntity>(length);
            var command = connection.GetRemoveCommand<TEntity>(sql, length);
            if (connection.CurrentTransaction != null)
                command.Transaction = connection.CurrentTransaction.Inner;

            cache.Execute(command, entities2);
            i += length;
        }
    }
}
