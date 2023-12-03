using LtQuery.Elements;
using LtQuery.Metadata;
using LtQuery.Relational.Generators;
using LtQuery.Relational.Generators.Asyncs;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace LtQuery.Relational;

class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    readonly EntityMetaService _metaService;
    readonly ISqlBuilder _sqlBuilder;
    readonly EntityMeta _meta;
    readonly ReadGenerator _generator;
    readonly AsyncGenerator _asyncGenerator;
    readonly InjectParameterGenerator _injectParameterGenerator;
    readonly InjectIdsGenerator _injectIdsGenerator;
    public Repository(EntityMetaService metaService, ISqlBuilder sqlBuilder)
    {
        _metaService = metaService;
        _sqlBuilder = sqlBuilder;
        _meta = _metaService.GetEntityMeta<TEntity>();
        _generator = new(_metaService);
        _asyncGenerator = new(_metaService);
        _injectParameterGenerator = new(_metaService);
        _injectIdsGenerator = new(_metaService);
    }


    class Cache2
    {
        public string Sql { get; }
        public ExecuteSelect<TEntity>? Read { get; set; }
        public ExecuteSelectAsync<TEntity>? ReadAsync { get; set; }
        public Cache2(string sql)
        {
            Sql = sql;
        }
    }

    class Cache
    {
        public Cache2? Select { get; set; }
        public Cache2? First { get; set; }
        public Cache2? Single { get; set; }
        public string? CountSql { get; set; }
    }

    readonly ConditionalWeakTable<Query<TEntity>, Cache> _caches = new();
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
            return cache;
        }
        finally
        {
            _cachesLocker.ExitUpgradeableReadLock();
        }
    }

    static class UpdateCache
    {
        public static InjectParameterForUpdate<TEntity>? Add { get; set; }
        public static InjectParameterForUpdate<TEntity>? Update { get; set; }
        public static InjectParameterForUpdate<TEntity>? Remove { get; set; }
        public static InjectIds<TEntity>? InjectIds { get; set; }
    }


    public IReadOnlyList<TEntity> Select(LtConnection connection, Query<TEntity> query)
    {
        if (connection.Inner.State == ConnectionState.Closed)
            connection.Inner.Open();

        var cache = getReaderCache(query);

        var cache2 = cache.Select;
        if (cache2 == null)
        {
            var sql = _sqlBuilder.CreateSelectSql(query);
            cache2 = new(sql);
            cache.Select = cache2;
        }
        var read = cache2.Read;
        if (read == null)
        {
            read = _generator.CreateReadSelectFunc(query);
            cache2.Read = read;
        }

        using (var command = createCommand(connection, cache2.Sql))
        {
            return read(command);
        }
    }

    public async ValueTask<IReadOnlyList<TEntity>> SelectAsync(LtConnection connection, Query<TEntity> query, CancellationToken cancellationToken = default)
    {
        if (connection.Inner.State == ConnectionState.Closed)
            await connection.Inner.OpenAsync(cancellationToken);

        var cache = getReaderCache(query);

        var cache2 = cache.Select;
        if (cache2 == null)
        {
            var sql = _sqlBuilder.CreateSelectSql(query);
            cache2 = new(sql);
            cache.Select = cache2;
        }
        var readAsync = cache2.ReadAsync;
        if (readAsync == null)
        {
            readAsync = _asyncGenerator.CreateReadSelectAsyncFunc(query);
            cache2.ReadAsync = readAsync;
        }

        var command = createCommand(connection, cache2.Sql);
        try
        {
            return await readAsync(command, cancellationToken);
        }
        finally
        {
            await command.DisposeAsync();
        }
    }

    public IReadOnlyList<TEntity> Select<TParameter>(LtConnection connection, Query<TEntity> query, TParameter values)
    {
        if (connection.Inner.State == ConnectionState.Closed)
            connection.Inner.Open();

        var cache = getReaderCache(query);

        var cache2 = cache.Select;
        if (cache2 == null)
        {
            var sql = _sqlBuilder.CreateSelectSql(query);
            cache2 = new(sql);
            cache.Select = cache2;
        }
        var read = cache2.Read;
        if (read == null)
        {
            read = _generator.CreateReadSelectFunc(query);
            cache2.Read = read;
        }

        var injectParameter = InjectParameterCache<TParameter>.Value;
        if (injectParameter == null)
        {
            injectParameter = _injectParameterGenerator.CreateInjectParameterFunc<TParameter>();
            InjectParameterCache<TParameter>.Value = injectParameter;
        }

        using (var command = createCommand(connection, cache2.Sql))
        {
            injectParameter(command, values);
            return read(command);
        }
    }

    public async ValueTask<IReadOnlyList<TEntity>> SelectAsync<TParameter>(LtConnection connection, Query<TEntity> query, TParameter values, CancellationToken cancellationToken = default)
    {
        if (connection.Inner.State == ConnectionState.Closed)
            await connection.Inner.OpenAsync(cancellationToken);

        var cache = getReaderCache(query);

        var cache2 = cache.Select;
        if (cache2 == null)
        {
            var sql = _sqlBuilder.CreateSelectSql(query);
            cache2 = new(sql);
            cache.Select = cache2;
        }
        var readAsync = cache2.ReadAsync;
        if (readAsync == null)
        {
            readAsync = _asyncGenerator.CreateReadSelectAsyncFunc(query);
            cache2.ReadAsync = readAsync;
        }

        var injectParameter = InjectParameterCache<TParameter>.Value;
        if (injectParameter == null)
        {
            injectParameter = _injectParameterGenerator.CreateInjectParameterFunc<TParameter>();
            InjectParameterCache<TParameter>.Value = injectParameter;
        }

        var command = createCommand(connection, cache2.Sql);
        try
        {
            injectParameter(command, values);
            return await readAsync(command, cancellationToken);
        }
        finally
        {
            await command.DisposeAsync();
        }
    }

    public TEntity Single(LtConnection connection, Query<TEntity> query)
    {
        if (connection.Inner.State == ConnectionState.Closed)
            connection.Inner.Open();

        var cache = getReaderCache(query);

        var signleQuery = new Query<TEntity>(query.Condition, query.Includes, query.OrderBys, query.SkipCount, new ConstantValue("2"));

        var cache2 = cache.Single;
        if (cache2 == null)
        {
            var sql = _sqlBuilder.CreateSelectSql(signleQuery);
            cache2 = new(sql);
            cache.Single = cache2;
        }
        var read = cache2.Read;
        if (read == null)
        {
            read = _generator.CreateReadSelectFunc(signleQuery);
            cache2.Read = read;
        }

        using (var command = createCommand(connection, cache2.Sql))
        {
            return read(command).Single();
        }
    }

    public async ValueTask<TEntity> SingleAsync(LtConnection connection, Query<TEntity> query, CancellationToken cancellationToken = default)
    {
        if (connection.Inner.State == ConnectionState.Closed)
            await connection.Inner.OpenAsync(cancellationToken);

        var cache = getReaderCache(query);

        var signleQuery = new Query<TEntity>(query.Condition, query.Includes, query.OrderBys, query.SkipCount, new ConstantValue("2"));

        var cache2 = cache.Single;
        if (cache2 == null)
        {
            var sql = _sqlBuilder.CreateSelectSql(signleQuery);
            cache2 = new(sql);
            cache.Single = cache2;
        }
        var readAsync = cache2.ReadAsync;
        if (readAsync == null)
        {
            readAsync = _asyncGenerator.CreateReadSelectAsyncFunc(signleQuery);
            cache2.ReadAsync = readAsync;
        }

        var command = createCommand(connection, cache2.Sql);
        try
        {
            return (await readAsync(command, cancellationToken)).Single();
        }
        finally
        {
            await command.DisposeAsync();
        }
    }

    public TEntity Single<TParameter>(LtConnection connection, Query<TEntity> query, TParameter values)
    {
        if (connection.Inner.State == ConnectionState.Closed)
            connection.Inner.Open();

        var cache = getReaderCache(query);

        var signleQuery = new Query<TEntity>(query.Condition, query.Includes, query.OrderBys, query.SkipCount, new ConstantValue("2"));

        var cache2 = cache.Single;
        if (cache2 == null)
        {
            var sql = _sqlBuilder.CreateSelectSql(signleQuery);
            cache2 = new(sql);
            cache.Single = cache2;
        }
        var read = cache2.Read;
        if (read == null)
        {
            read = _generator.CreateReadSelectFunc(signleQuery);
            cache2.Read = read;
        }

        var injectParameter = InjectParameterCache<TParameter>.Value;
        if (injectParameter == null)
        {
            injectParameter = _injectParameterGenerator.CreateInjectParameterFunc<TParameter>();
            InjectParameterCache<TParameter>.Value = injectParameter;
        }

        using (var command = createCommand(connection, cache2.Sql))
        {
            injectParameter(command, values);
            return read(command).Single();
        }
    }

    public async ValueTask<TEntity> SingleAsync<TParameter>(LtConnection connection, Query<TEntity> query, TParameter values, CancellationToken cancellationToken = default)
    {
        if (connection.Inner.State == ConnectionState.Closed)
            await connection.Inner.OpenAsync(cancellationToken);

        var cache = getReaderCache(query);

        var signleQuery = new Query<TEntity>(query.Condition, query.Includes, query.OrderBys, query.SkipCount, new ConstantValue("2"));

        var cache2 = cache.Single;
        if (cache2 == null)
        {
            var sql = _sqlBuilder.CreateSelectSql(signleQuery);
            cache2 = new(sql);
            cache.Single = cache2;
        }
        var readAsync = cache2.ReadAsync;
        if (readAsync == null)
        {
            readAsync = _asyncGenerator.CreateReadSelectAsyncFunc(signleQuery);
            cache2.ReadAsync = readAsync;
        }

        var injectParameter = InjectParameterCache<TParameter>.Value;
        if (injectParameter == null)
        {
            injectParameter = _injectParameterGenerator.CreateInjectParameterFunc<TParameter>();
            InjectParameterCache<TParameter>.Value = injectParameter;
        }

        var command = createCommand(connection, cache2.Sql);
        try
        {
            injectParameter(command, values);
            return (await readAsync(command, cancellationToken)).Single();
        }
        finally
        {
            await command.DisposeAsync();
        }
    }

    public TEntity First(LtConnection connection, Query<TEntity> query)
    {
        if (connection.Inner.State == ConnectionState.Closed)
            connection.Inner.Open();

        var cache = getReaderCache(query);

        var firstQuery = new Query<TEntity>(query.Condition, query.Includes, query.OrderBys, query.SkipCount, new ConstantValue("1"));

        var cache2 = cache.First;
        if (cache2 == null)
        {
            var sql = _sqlBuilder.CreateSelectSql(firstQuery);
            cache2 = new(sql);
            cache.First = cache2;
        }
        var read = cache2.Read;
        if (read == null)
        {
            read = _generator.CreateReadSelectFunc(firstQuery);
            cache2.Read = read;
        }

        using (var command = createCommand(connection, cache2.Sql))
        {
            return read(command).First();
        }
    }

    public async ValueTask<TEntity> FirstAsync(LtConnection connection, Query<TEntity> query, CancellationToken cancellationToken = default)
    {
        if (connection.Inner.State == ConnectionState.Closed)
            await connection.Inner.OpenAsync(cancellationToken);

        var cache = getReaderCache(query);

        var firstQuery = new Query<TEntity>(query.Condition, query.Includes, query.OrderBys, query.SkipCount, new ConstantValue("1"));

        var cache2 = cache.First;
        if (cache2 == null)
        {
            var sql = _sqlBuilder.CreateSelectSql(firstQuery);
            cache2 = new(sql);
            cache.First = cache2;
        }
        var readAsync = cache2.ReadAsync;
        if (readAsync == null)
        {
            readAsync = _asyncGenerator.CreateReadSelectAsyncFunc(firstQuery);
            cache2.ReadAsync = readAsync;
        }

        var command = createCommand(connection, cache2.Sql);
        try
        {
            return (await readAsync(command, cancellationToken)).First();
        }
        finally
        {
            await command.DisposeAsync();
        }
    }

    public TEntity First<TParameter>(LtConnection connection, Query<TEntity> query, TParameter values)
    {
        if (connection.Inner.State == ConnectionState.Closed)
            connection.Inner.Open();

        var cache = getReaderCache(query);

        var firstQuery = new Query<TEntity>(query.Condition, query.Includes, query.OrderBys, query.SkipCount, new ConstantValue("1"));

        var cache2 = cache.First;
        if (cache2 == null)
        {
            var sql = _sqlBuilder.CreateSelectSql(firstQuery);
            cache2 = new(sql);
            cache.First = cache2;
        }
        var read = cache2.Read;
        if (read == null)
        {
            read = _generator.CreateReadSelectFunc(firstQuery);
            cache2.Read = read;
        }

        var injectParameter = InjectParameterCache<TParameter>.Value;
        if (injectParameter == null)
        {
            injectParameter = _injectParameterGenerator.CreateInjectParameterFunc<TParameter>();
            InjectParameterCache<TParameter>.Value = injectParameter;
        }

        using (var command = createCommand(connection, cache2.Sql))
        {
            injectParameter(command, values);
            return read(command).First();
        }
    }

    public async ValueTask<TEntity> FirstAsync<TParameter>(LtConnection connection, Query<TEntity> query, TParameter values, CancellationToken cancellationToken = default)
    {
        if (connection.Inner.State == ConnectionState.Closed)
            await connection.Inner.OpenAsync(cancellationToken);

        var cache = getReaderCache(query);

        var firstQuery = new Query<TEntity>(query.Condition, query.Includes, query.OrderBys, query.SkipCount, new ConstantValue("1"));

        var cache2 = cache.First;
        if (cache2 == null)
        {
            var sql = _sqlBuilder.CreateSelectSql(firstQuery);
            cache2 = new(sql);
            cache.First = cache2;
        }
        var readAsync = cache2.ReadAsync;
        if (readAsync == null)
        {
            readAsync = _asyncGenerator.CreateReadSelectAsyncFunc(firstQuery);
            cache2.ReadAsync = readAsync;
        }

        var injectParameter = InjectParameterCache<TParameter>.Value;
        if (injectParameter == null)
        {
            injectParameter = _injectParameterGenerator.CreateInjectParameterFunc<TParameter>();
            InjectParameterCache<TParameter>.Value = injectParameter;
        }

        var command = createCommand(connection, cache2.Sql);
        try
        {
            injectParameter(command, values);
            return (await readAsync(command, cancellationToken)).First();
        }
        finally
        {
            await command.DisposeAsync();
        }
    }

    public int Count(LtConnection connection, Query<TEntity> query)
    {
        if (connection.Inner.State == ConnectionState.Closed)
            connection.Inner.Open();

        var cache = getReaderCache(query);

        var sql = cache.CountSql;
        if (sql == null)
        {
            sql = _sqlBuilder.CreateCountSql(query);
            cache.CountSql = sql;
        }

        using (var command = createCommand(connection, sql))
        {
            return executeReadInt(command);
        }
    }

    public async ValueTask<int> CountAsync(LtConnection connection, Query<TEntity> query, CancellationToken cancellationToken = default)
    {
        if (connection.Inner.State == ConnectionState.Closed)
            await connection.Inner.OpenAsync(cancellationToken);

        var cache = getReaderCache(query);

        var sql = cache.CountSql;
        if (sql == null)
        {
            sql = _sqlBuilder.CreateCountSql(query);
            cache.CountSql = sql;
        }

        var command = createCommand(connection, sql);
        try
        {
            return await executeReadIntAsync(command, cancellationToken);
        }
        finally
        {
            await command.DisposeAsync();
        }
    }

    public int Count<TParameter>(LtConnection connection, Query<TEntity> query, TParameter values)
    {
        if (connection.Inner.State == ConnectionState.Closed)
            connection.Inner.Open();

        var cache = getReaderCache(query);

        var sql = cache.CountSql;
        if (sql == null)
        {
            sql = _sqlBuilder.CreateCountSql(query);
            cache.CountSql = sql;
        }

        var injectParameter = InjectParameterCache<TParameter>.Value;
        if (injectParameter == null)
        {
            injectParameter = _injectParameterGenerator.CreateInjectParameterFunc<TParameter>();
            InjectParameterCache<TParameter>.Value = injectParameter;
        }

        using (var command = createCommand(connection, sql))
        {
            injectParameter(command, values);
            return executeReadInt(command);
        }
    }

    public async ValueTask<int> CountAsync<TParameter>(LtConnection connection, Query<TEntity> query, TParameter values, CancellationToken cancellationToken = default)
    {
        if (connection.Inner.State == ConnectionState.Closed)
            await connection.Inner.OpenAsync(cancellationToken);

        var cache = getReaderCache(query);

        var sql = cache.CountSql;
        if (sql == null)
        {
            sql = _sqlBuilder.CreateCountSql(query);
            cache.CountSql = sql;
        }

        var injectParameter = InjectParameterCache<TParameter>.Value;
        if (injectParameter == null)
        {
            injectParameter = _injectParameterGenerator.CreateInjectParameterFunc<TParameter>();
            InjectParameterCache<TParameter>.Value = injectParameter;
        }

        var command = createCommand(connection, sql);
        try
        {
            injectParameter(command, values);
            return await executeReadIntAsync(command, cancellationToken);
        }
        finally
        {
            await command.DisposeAsync();
        }
    }

    const int _maxParameterCount = 2099;

    public void Add(LtConnection connection, Span<TEntity> entities)
    {
        if (connection.Inner.State == ConnectionState.Closed)
            connection.Inner.Open();

        var injectIds = UpdateCache.InjectIds;
        if (injectIds == null)
        {
            injectIds = _injectIdsGenerator.CreateInjectParametersFunc<TEntity>();
            UpdateCache.InjectIds = injectIds;
        }
        var injectParameter = UpdateCache.Add;
        if (injectParameter == null)
        {
            injectParameter = _injectParameterGenerator.CreateInjectParameterForUpdateFunc<TEntity>(DbMethod.Add);
            UpdateCache.Add = injectParameter;
        }

        var maxLength = _maxParameterCount / _meta.Properties.Count;
        var i = 0;
        var count = entities.Length;
        while (i < count)
        {
            var length = count - i;
            if (length > maxLength)
                length = maxLength;
            var entities2 = entities.Slice(i, length);

            var sql = _sqlBuilder.CreateAddSql<TEntity>(length);
            using (var command = createCommand(connection, sql))
            {
                injectParameter(command, entities2);
                var ids = executeReadInts(command, length);
                injectIds(entities2, ids);
            }
            i += length;
        }
    }

    public async ValueTask AddAsync(LtConnection connection, TEntity[] entities, CancellationToken cancellationToken = default)
    {
        if (connection.Inner.State == ConnectionState.Closed)
            connection.Inner.Open();

        var injectIds = UpdateCache.InjectIds;
        if (injectIds == null)
        {
            injectIds = _injectIdsGenerator.CreateInjectParametersFunc<TEntity>();
            UpdateCache.InjectIds = injectIds;
        }
        var injectParameter = UpdateCache.Add;
        if (injectParameter == null)
        {
            injectParameter = _injectParameterGenerator.CreateInjectParameterForUpdateFunc<TEntity>(DbMethod.Add);
            UpdateCache.Add = injectParameter;
        }

        var maxLength = _maxParameterCount / _meta.Properties.Count;
        var i = 0;
        var count = entities.Length;
        while (i < count)
        {
            var length = count - i;
            if (length > maxLength)
                length = maxLength;
            var entities2 = entities[i..(i + length)];

            var sql = _sqlBuilder.CreateAddSql<TEntity>(length);
            using (var command = createCommand(connection, sql))
            {
                injectParameter(command, entities2);
                var ids = await executeReadIntsAsync(command, length, cancellationToken);
                injectIds(entities2, ids);
            }
            i += length;
        }
    }

    public void Update(LtConnection connection, Span<TEntity> entities)
    {
        if (connection.Inner.State == ConnectionState.Closed)
            connection.Inner.Open();

        var injectParameter = UpdateCache.Update;
        if (injectParameter == null)
        {
            injectParameter = _injectParameterGenerator.CreateInjectParameterForUpdateFunc<TEntity>(DbMethod.Update);
            UpdateCache.Update = injectParameter;
        }

        var maxLength = _maxParameterCount / _meta.Properties.Count;
        var i = 0;
        var count = entities.Length;
        while (i < count)
        {
            var length = count - i;
            if (length > maxLength)
                length = maxLength;
            var entities2 = entities.Slice(i, length);

            var sql = _sqlBuilder.CreateUpdatedSql<TEntity>(length);
            using (var command = createCommand(connection, sql))
            {
                injectParameter(command, entities2);
                command.ExecuteNonQuery();
            }
            i += length;
        }
    }

    public async ValueTask UpdateAsync(LtConnection connection, TEntity[] entities, CancellationToken cancellationToken = default)
    {
        if (connection.Inner.State == ConnectionState.Closed)
            await connection.Inner.OpenAsync(cancellationToken);

        var injectParameter = UpdateCache.Update;
        if (injectParameter == null)
        {
            injectParameter = _injectParameterGenerator.CreateInjectParameterForUpdateFunc<TEntity>(DbMethod.Update);
            UpdateCache.Update = injectParameter;
        }

        var maxLength = _maxParameterCount / _meta.Properties.Count;
        var i = 0;
        var count = entities.Length;
        while (i < count)
        {
            var length = count - i;
            if (length > maxLength)
                length = maxLength;
            var entities2 = entities[i..(i + length)];

            var sql = _sqlBuilder.CreateUpdatedSql<TEntity>(length);
            var command = createCommand(connection, sql);
            try
            {
                injectParameter(command, entities2);
                await command.ExecuteNonQueryAsync(cancellationToken);
            }
            finally
            {
                await command.DisposeAsync();
            }
            i += length;
        }
    }

    public void Remove(LtConnection connection, Span<TEntity> entities)
    {
        if (connection.Inner.State == ConnectionState.Closed)
            connection.Inner.Open();

        var injectParameter = UpdateCache.Remove;
        if (injectParameter == null)
        {
            injectParameter = _injectParameterGenerator.CreateInjectParameterForUpdateFunc<TEntity>(DbMethod.Remove);
            UpdateCache.Remove = injectParameter;
        }

        var maxLength = _maxParameterCount / _meta.Properties.Count;
        var i = 0;
        var count = entities.Length;
        while (i < count)
        {
            var length = count - i;
            if (length > maxLength)
                length = maxLength;
            var entities2 = entities.Slice(i, length);

            var sql = _sqlBuilder.CreateRemoveSql<TEntity>(length);
            using (var command = createCommand(connection, sql))
            {
                injectParameter(command, entities2);
                command.ExecuteNonQuery();
            }
            i += length;
        }
    }

    public async ValueTask RemoveAsync(LtConnection connection, TEntity[] entities, CancellationToken cancellationToken = default)
    {
        if (connection.Inner.State == ConnectionState.Closed)
            await connection.Inner.OpenAsync(cancellationToken);

        var injectParameter = UpdateCache.Remove;
        if (injectParameter == null)
        {
            injectParameter = _injectParameterGenerator.CreateInjectParameterForUpdateFunc<TEntity>(DbMethod.Remove);
            UpdateCache.Remove = injectParameter;
        }

        var count = entities.Length;

        var maxLength = _maxParameterCount / _meta.Properties.Count;
        var i = 0;
        while (i < count)
        {
            var length = count - i;
            if (length > maxLength)
                length = maxLength;
            var entities2 = entities[i..(i + length)];

            var sql = _sqlBuilder.CreateRemoveSql<TEntity>(length);
            var command = createCommand(connection, sql);
            try
            {
                injectParameter(command, entities2);
                await command.ExecuteNonQueryAsync(cancellationToken);
            }
            finally
            {
                await command.DisposeAsync();
            }
            i += length;
        }
    }


    static int executeReadInt(DbCommand command)
    {
        using (var reader = command.ExecuteReader())
        {
            reader.Read();
            return reader.GetInt32(0);
        }
    }
    static async ValueTask<int> executeReadIntAsync(DbCommand command, CancellationToken cancellationToken)
    {
        var reader = await command.ExecuteReaderAsync(cancellationToken);
        try
        {
            await reader.ReadAsync();
            return reader.GetInt32(0);
        }
        finally
        {
            await reader.DisposeAsync();
        }
    }

    static int[] executeReadInts(DbCommand command, int count)
    {
        using (var reader = command.ExecuteReader())
        {
            int index = 0;
            var array = new int[count];
            var span = array.AsSpan();
            while (reader.Read())
            {
                span[index++] = reader.GetInt32(0);
            }

            return array;
        }
    }
    static async ValueTask<int[]> executeReadIntsAsync(DbCommand command, int count, CancellationToken cancellationToken)
    {
        var reader = await command.ExecuteReaderAsync(cancellationToken);
        try
        {
            int index = 0;
            var array = new int[count];
            while (await reader.ReadAsync())
            {
                array[index++] = reader.GetInt32(0);
            }

            return array;
        }
        finally
        {
            await reader.DisposeAsync();
        }
    }

    static DbCommand createCommand(LtConnection connection, string sql)
    {
        var command = connection.Inner.CreateCommand();
        command.CommandText = sql;
        command.Transaction = connection.CurrentTransaction?.Inner;
        return command;
    }
}
