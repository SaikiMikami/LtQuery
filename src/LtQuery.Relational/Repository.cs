using LtQuery.Elements;
using LtQuery.Metadata;
using LtQuery.Relational.Generators;
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
    readonly AddGenerator _addGenerator;
    readonly InjectParameterGenerator _injectParameterGenerator;
    public Repository(EntityMetaService metaService, ISqlBuilder sqlBuilder)
    {
        _metaService = metaService;
        _sqlBuilder = sqlBuilder;
        _meta = _metaService.GetEntityMeta<TEntity>();
        _generator = new(_metaService);
        _addGenerator = new(_metaService);
        _injectParameterGenerator = new(_metaService);
    }


    class Cache2
    {
        public string Sql { get; }
        public ExecuteSelect<TEntity> Read { get; }
        public Cache2(string sql, ExecuteSelect<TEntity> read)
        {
            Sql = sql;
            Read = read;
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
        public static ExecuteAdd<TEntity>? ExecuteAdd { get; set; }
    }

    Cache2 getSelectCache(Query<TEntity> query)
    {
        var cache = getReaderCache(query);

        var cache2 = cache.Select;
        if (cache2 == null)
        {
            var sql = _sqlBuilder.CreateSelectSql(query);
            var read = _generator.CreateReadSelectFunc(query);
            cache2 = new(sql, read);
            cache.Select = cache2;
        }
        return cache2;
    }

    Cache2 getSingleCache(Query<TEntity> query)
    {
        var cache = getReaderCache(query);

        var cache2 = cache.Single;
        if (cache2 == null)
        {
            var signleQuery = new Query<TEntity>(query.Condition, query.Includes, query.OrderBys, query.SkipCount, new ConstantValue("2"));
            var sql = _sqlBuilder.CreateSelectSql(signleQuery);
            var read = _generator.CreateReadSelectFunc(signleQuery);
            cache2 = new(sql, read);
            cache.Single = cache2;
        }
        return cache2;
    }

    Cache2 getFirstCache(Query<TEntity> query)
    {
        var cache = getReaderCache(query);

        var cache2 = cache.First;
        if (cache2 == null)
        {
            var firstQuery = new Query<TEntity>(query.Condition, query.Includes, query.OrderBys, query.SkipCount, new ConstantValue("1"));
            var sql = _sqlBuilder.CreateSelectSql(firstQuery);
            var read = _generator.CreateReadSelectFunc(firstQuery);
            cache2 = new(sql, read);
            cache.First = cache2;
        }
        return cache2;
    }

    string getCountCache(Query<TEntity> query)
    {
        var cache = getReaderCache(query);

        var sql = cache.CountSql;
        if (sql == null)
        {
            sql = _sqlBuilder.CreateCountSql(query);
            cache.CountSql = sql;
        }
        return sql;
    }

    public IReadOnlyList<TEntity> Select(LtConnection connection, Query<TEntity> query)
    {
        if (connection.Inner.State == ConnectionState.Closed)
            connection.Inner.Open();

        var cache = getSelectCache(query);

        using var command = createCommand(connection, cache.Sql);
        using var reader = command.ExecuteReader();

        return cache.Read(reader);
    }

    public async ValueTask<IReadOnlyList<TEntity>> SelectAsync(LtConnection connection, Query<TEntity> query, CancellationToken cancellationToken = default)
    {
        if (connection.Inner.State == ConnectionState.Closed)
            await connection.Inner.OpenAsync(cancellationToken);

        var cache = getSelectCache(query);

        using var command = createCommand(connection, cache.Sql);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return cache.Read(reader);
    }

    public IReadOnlyList<TEntity> Select<TParameter>(LtConnection connection, Query<TEntity> query, TParameter values)
    {
        if (connection.Inner.State == ConnectionState.Closed)
            connection.Inner.Open();

        var cache = getSelectCache(query);

        var injectParameter = InjectParameterCache<TParameter>.GetValue(_injectParameterGenerator);

        using var command = createCommand(connection, cache.Sql);

        injectParameter(command, values);

        using var reader = command.ExecuteReader();

        return cache.Read(reader);
    }

    public async ValueTask<IReadOnlyList<TEntity>> SelectAsync<TParameter>(LtConnection connection, Query<TEntity> query, TParameter values, CancellationToken cancellationToken = default)
    {
        if (connection.Inner.State == ConnectionState.Closed)
            await connection.Inner.OpenAsync(cancellationToken);

        var cache = getSelectCache(query);

        var injectParameter = InjectParameterCache<TParameter>.GetValue(_injectParameterGenerator);

        using var command = createCommand(connection, cache.Sql);

        injectParameter(command, values);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return cache.Read(reader);
    }

    public TEntity Single(LtConnection connection, Query<TEntity> query)
    {
        if (connection.Inner.State == ConnectionState.Closed)
            connection.Inner.Open();

        var cache = getSingleCache(query);

        using var command = createCommand(connection, cache.Sql);
        using var reader = command.ExecuteReader();

        return cache.Read(reader).Single();
    }

    public async ValueTask<TEntity> SingleAsync(LtConnection connection, Query<TEntity> query, CancellationToken cancellationToken = default)
    {
        if (connection.Inner.State == ConnectionState.Closed)
            await connection.Inner.OpenAsync(cancellationToken);

        var cache = getSingleCache(query);

        using var command = createCommand(connection, cache.Sql);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return cache.Read(reader).Single();
    }

    public TEntity Single<TParameter>(LtConnection connection, Query<TEntity> query, TParameter values)
    {
        if (connection.Inner.State == ConnectionState.Closed)
            connection.Inner.Open();

        var cache = getSingleCache(query);

        var injectParameter = InjectParameterCache<TParameter>.GetValue(_injectParameterGenerator);

        using var command = createCommand(connection, cache.Sql);

        injectParameter(command, values);

        using var reader = command.ExecuteReader();

        return cache.Read(reader).Single();
    }

    public async ValueTask<TEntity> SingleAsync<TParameter>(LtConnection connection, Query<TEntity> query, TParameter values, CancellationToken cancellationToken = default)
    {
        if (connection.Inner.State == ConnectionState.Closed)
            await connection.Inner.OpenAsync(cancellationToken);

        var cache = getSingleCache(query);

        var injectParameter = InjectParameterCache<TParameter>.GetValue(_injectParameterGenerator);

        using var command = createCommand(connection, cache.Sql);

        injectParameter(command, values);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return cache.Read(reader).Single();
    }

    public TEntity First(LtConnection connection, Query<TEntity> query)
    {
        if (connection.Inner.State == ConnectionState.Closed)
            connection.Inner.Open();

        var cache = getFirstCache(query);

        using var command = createCommand(connection, cache.Sql);
        using var reader = command.ExecuteReader();

        return cache.Read(reader).First();
    }

    public async ValueTask<TEntity> FirstAsync(LtConnection connection, Query<TEntity> query, CancellationToken cancellationToken = default)
    {
        if (connection.Inner.State == ConnectionState.Closed)
            await connection.Inner.OpenAsync(cancellationToken);

        var cache = getFirstCache(query);

        using var command = createCommand(connection, cache.Sql);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return cache.Read(reader).First();
    }

    public TEntity First<TParameter>(LtConnection connection, Query<TEntity> query, TParameter values)
    {
        if (connection.Inner.State == ConnectionState.Closed)
            connection.Inner.Open();

        var cache = getFirstCache(query);

        var injectParameter = InjectParameterCache<TParameter>.GetValue(_injectParameterGenerator);

        using var command = createCommand(connection, cache.Sql);

        injectParameter(command, values);

        using var reader = command.ExecuteReader();

        return cache.Read(reader).First();
    }

    public async ValueTask<TEntity> FirstAsync<TParameter>(LtConnection connection, Query<TEntity> query, TParameter values, CancellationToken cancellationToken = default)
    {
        if (connection.Inner.State == ConnectionState.Closed)
            await connection.Inner.OpenAsync(cancellationToken);

        var cache = getFirstCache(query);

        var injectParameter = InjectParameterCache<TParameter>.GetValue(_injectParameterGenerator);

        using var command = createCommand(connection, cache.Sql);

        injectParameter(command, values);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return cache.Read(reader).First();
    }

    public int Count(LtConnection connection, Query<TEntity> query)
    {
        if (connection.Inner.State == ConnectionState.Closed)
            connection.Inner.Open();

        var sql = getCountCache(query);

        using var command = createCommand(connection, sql);
        using var reader = command.ExecuteReader();

        return executeReadInt(reader);
    }

    public async ValueTask<int> CountAsync(LtConnection connection, Query<TEntity> query, CancellationToken cancellationToken = default)
    {
        if (connection.Inner.State == ConnectionState.Closed)
            await connection.Inner.OpenAsync(cancellationToken);

        var sql = getCountCache(query);

        using var command = createCommand(connection, sql);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return executeReadInt(reader);
    }

    public int Count<TParameter>(LtConnection connection, Query<TEntity> query, TParameter values)
    {
        if (connection.Inner.State == ConnectionState.Closed)
            connection.Inner.Open();

        var sql = getCountCache(query);

        var injectParameter = InjectParameterCache<TParameter>.GetValue(_injectParameterGenerator);

        using var command = createCommand(connection, sql);

        injectParameter(command, values);

        using var reader = command.ExecuteReader();

        return executeReadInt(reader);
    }

    public async ValueTask<int> CountAsync<TParameter>(LtConnection connection, Query<TEntity> query, TParameter values, CancellationToken cancellationToken = default)
    {
        if (connection.Inner.State == ConnectionState.Closed)
            await connection.Inner.OpenAsync(cancellationToken);

        var sql = getCountCache(query);

        var injectParameter = InjectParameterCache<TParameter>.GetValue(_injectParameterGenerator);

        using var command = createCommand(connection, sql);

        injectParameter(command, values);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return executeReadInt(reader);
    }

    ExecuteAdd<TEntity> getAddCache()
    {
        var executeAdd = UpdateCache.ExecuteAdd;
        if (executeAdd == null)
        {
            executeAdd = _addGenerator.CreateInjectParametersFunc<TEntity>();
            UpdateCache.ExecuteAdd = executeAdd;
        }
        return executeAdd;
    }

    InjectParameterForUpdate<TEntity> getInjectParameterCache(DbMethod method)
    {
        switch (method)
        {
            case DbMethod.Add:
                var injectParameter = UpdateCache.Add;
                if (injectParameter == null)
                {
                    injectParameter = _injectParameterGenerator.CreateInjectParameterForUpdateFunc<TEntity>(method);
                    UpdateCache.Add = injectParameter;
                }
                return injectParameter;

            case DbMethod.Update:
                injectParameter = UpdateCache.Update;
                if (injectParameter == null)
                {
                    injectParameter = _injectParameterGenerator.CreateInjectParameterForUpdateFunc<TEntity>(method);
                    UpdateCache.Update = injectParameter;
                }
                return injectParameter;

            case DbMethod.Remove:
                injectParameter = UpdateCache.Remove;
                if (injectParameter == null)
                {
                    injectParameter = _injectParameterGenerator.CreateInjectParameterForUpdateFunc<TEntity>(method);
                    UpdateCache.Remove = injectParameter;
                }
                return injectParameter;
            default:
                throw new InvalidProgramException();
        }
    }

    const int _maxParameterCount = 2099;

    public void Add(LtConnection connection, Span<TEntity> entities)
    {
        if (connection.Inner.State == ConnectionState.Closed)
            connection.Inner.Open();

        var executeAdd = getAddCache();

        var injectParameter = getInjectParameterCache(DbMethod.Add);

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

                using var reader = command.ExecuteReader();

                executeAdd(reader, entities2);
            }
            i += length;
        }
    }

    public async ValueTask AddAsync(LtConnection connection, TEntity[] entities, CancellationToken cancellationToken = default)
    {
        if (connection.Inner.State == ConnectionState.Closed)
            connection.Inner.Open();

        var executeAdd = getAddCache();

        var injectParameter = getInjectParameterCache(DbMethod.Add);

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

                using var reader = await command.ExecuteReaderAsync(cancellationToken);
                executeAdd(reader, entities2);
            }
            i += length;
        }
    }

    public void Update(LtConnection connection, Span<TEntity> entities)
    {
        if (connection.Inner.State == ConnectionState.Closed)
            connection.Inner.Open();

        var injectParameter = getInjectParameterCache(DbMethod.Update);

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

        var injectParameter = getInjectParameterCache(DbMethod.Update);

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

        var injectParameter = getInjectParameterCache(DbMethod.Remove);

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

        var injectParameter = getInjectParameterCache(DbMethod.Remove);

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


    static int executeReadInt(DbDataReader reader)
    {
        reader.Read();
        return reader.GetInt32(0);
    }

    static int[] executeReadInts(DbDataReader reader, int count)
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

    static DbCommand createCommand(LtConnection connection, string sql)
    {
        var command = connection.Inner.CreateCommand();
        command.CommandText = sql;
        command.Transaction = connection.CurrentTransaction?.Inner;
        return command;
    }
}
