using LtQuery.Elements;
using LtQuery.Metadata;
using LtQuery.Relational.Generators;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace LtQuery.Relational;

class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    readonly EntityMetaService _metaService;
    public Repository(EntityMetaService metaService)
    {
        _metaService = metaService;
    }

    interface IReaderCache { }
    class ReaderCache : IReaderCache
    {
        public Func<DbCommand, IReadOnlyList<TEntity>>? Select { get; set; }
        public Func<DbCommand, IReadOnlyList<TEntity>>? First { get; set; }
        public Func<DbCommand, IReadOnlyList<TEntity>>? Single { get; set; }
    }

    class ParameterReaderCache<TParameter> : IReaderCache
    {
        public Func<DbCommand, TParameter, IReadOnlyList<TEntity>>? Select { get; set; }
        public Func<DbCommand, TParameter, IReadOnlyList<TEntity>>? First { get; set; }
        public Func<DbCommand, TParameter, IReadOnlyList<TEntity>>? Single { get; set; }
    }

    ConditionalWeakTable<Query<TEntity>, IReaderCache> _caches = new();

    ReaderCache getReaderCache(Query<TEntity> query)
    {
        if (!_caches.TryGetValue(query, out var cache))
        {
            cache = new ReaderCache();
            _caches.Add(query, cache);
        }
        return (ReaderCache)cache;
    }
    ParameterReaderCache<TParameter> getReaderCache<TParameter>(Query<TEntity> query)
    {
        if (!_caches.TryGetValue(query, out var cache))
        {
            cache = new ParameterReaderCache<TParameter>();
            _caches.Add(query, cache);
        }
        return (ParameterReaderCache<TParameter>)cache;
    }

    public IReadOnlyList<TEntity> Select(LtConnection connection, Query<TEntity> query)
    {
        var cache = getReaderCache(query);

        var read = cache.Select;
        if (read == null)
        {
            read = new ReadGenerator<TEntity>(_metaService).CreateReadSelectFunc(query);
            cache.Select = read;
        }

        var command = connection.GetSelectCommand(query);

        return read(command);
    }

    public IReadOnlyList<TEntity> Select<TParameter>(LtConnection connection, Query<TEntity> query, TParameter values)
    {
        var cache = getReaderCache<TParameter>(query);

        var read = cache.Select;
        if (read == null)
        {
            read = new ReadGenerator<TEntity>(_metaService).CreateReadSelectFunc<TParameter>(query);
            cache.Select = read;
        }

        var commands = connection.GetSelectCommand(query);

        return read(commands, values);
    }

    public TEntity Single(LtConnection connection, Query<TEntity> query)
    {
        var cache = getReaderCache(query);

        var read = cache.Single;
        if (read == null)
        {
            var signleQuery = new Query<TEntity>(query.Condition, query.Includes, query.OrderBys, query.SkipCount, new ConstantValue("2"));
            read = new ReadGenerator<TEntity>(_metaService).CreateReadSelectFunc(signleQuery);
            cache.Single = read;
        }

        var commands = connection.GetSingleCommand(query);

        return read(commands).Single();
    }

    public TEntity Single<TParameter>(LtConnection connection, Query<TEntity> query, TParameter values)
    {
        var cache = getReaderCache<TParameter>(query);

        var read = cache.Single;
        if (read == null)
        {
            var signleQuery = new Query<TEntity>(query.Condition, query.Includes, query.OrderBys, query.SkipCount, new ConstantValue("2"));
            read = new ReadGenerator<TEntity>(_metaService).CreateReadSelectFunc<TParameter>(signleQuery);
            cache.Single = read;
        }

        var commands = connection.GetSingleCommand(query);

        return read(commands, values).Single();
    }

    public TEntity First(LtConnection connection, Query<TEntity> query)
    {
        var cache = getReaderCache(query);

        var read = cache.First;
        if (read == null)
        {
            var firstQuery = new Query<TEntity>(query.Condition, query.Includes, query.OrderBys, query.SkipCount, new ConstantValue("1"));
            read = new ReadGenerator<TEntity>(_metaService).CreateReadSelectFunc(firstQuery);
            cache.First = read;
        }

        var commands = connection.GetFirstCommand(query);

        return read(commands).First();
    }

    public TEntity First<TParameter>(LtConnection connection, Query<TEntity> query, TParameter values)
    {
        var cache = getReaderCache<TParameter>(query);

        var read = cache.First;
        if (read == null)
        {
            var firstQuery = new Query<TEntity>(query.Condition, query.Includes, query.OrderBys, query.SkipCount, new ConstantValue("1"));
            read = new ReadGenerator<TEntity>(_metaService).CreateReadSelectFunc<TParameter>(firstQuery);
            cache.First = read;
        }

        var commands = connection.GetFirstCommand(query);

        return read(commands, values).First();
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
