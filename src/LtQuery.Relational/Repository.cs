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
    readonly ReaderWriterLockSlim _locker = new();

    Cache getReaderCache(Query<TEntity> query)
    {
        _locker.EnterUpgradeableReadLock();
        try
        {
            if (!_caches.TryGetValue(query, out var cache))
            {
                _locker.EnterWriteLock();
                try
                {
                    cache = new Cache();
                    _caches.Add(query, cache);
                }
                finally
                {
                    _locker.ExitWriteLock();
                }
            }
            return (Cache)cache;
        }
        finally
        {
            _locker.ExitUpgradeableReadLock();
        }
    }
    Cache<TParameter> getReaderCache<TParameter>(Query<TEntity> query)
    {
        _locker.EnterUpgradeableReadLock();
        try
        {
            if (!_caches.TryGetValue(query, out var cache))
            {
                _locker.EnterWriteLock();
                try
                {
                    cache = new Cache<TParameter>();
                    _caches.Add(query, cache);
                }
                finally
                {
                    _locker.ExitWriteLock();
                }
            }
            return (Cache<TParameter>)cache;
        }
        finally
        {
            _locker.ExitUpgradeableReadLock();
        }
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

        var commands = connection.GetSelectCommand(query, cache2.Sql);

        return cache2.Read(commands, values);
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

        var commands = connection.GetSingleCommand(query, cache2.Sql);

        return cache2.Read(commands).Single();
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

        var commands = connection.GetSingleCommand(query, cache2.Sql);

        return cache2.Read(commands, values).Single();
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

        var commands = connection.GetFirstCommand(query, cache2.Sql);

        return cache2.Read(commands).First();
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

        var commands = connection.GetFirstCommand(query, cache2.Sql);

        return cache2.Read(commands, values).First();
    }

    public int Count(LtConnection connection, Query<TEntity> query)
    {
        throw new NotImplementedException();
    }

    public int Count<TParameter>(LtConnection connection, Query<TEntity> query, TParameter values)
    {
        throw new NotImplementedException();
    }
}
