using LtQuery.Elements;
using LtQuery.Elements.Values;
using LtQuery.Metadata;
using LtQuery.Sql.Generators;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace LtQuery.Sql;

class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    readonly EntityMetaService _metaService;
    readonly ISqlBuilder _sqlBuilder;
    public Repository(EntityMetaService metaService, ISqlBuilder sqlBuilder)
    {
        _metaService = metaService;
        _sqlBuilder = sqlBuilder;
    }

    class CacheEnty : IDisposable
    {
        public IReadOnlyList<DbCommand> Commands { get; }
        public CacheEnty(IReadOnlyList<DbCommand> commands)
        {
            Commands = commands;
        }

        public virtual void Dispose()
        {
            foreach (var command in Commands)
                command.Dispose();
        }
    }
    class SelectCacheEnty : CacheEnty
    {
        public Func<IReadOnlyList<DbCommand>, IReadOnlyList<TEntity>> Read { get; }
        public SelectCacheEnty(IReadOnlyList<DbCommand> commands, Func<IReadOnlyList<DbCommand>, IReadOnlyList<TEntity>> read) : base(commands)
        {
            Read = read;
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
    class SelectWithParametersCacheEnty<TParameter> : CacheEnty
    {
        public Func<IReadOnlyList<DbCommand>, TParameter, IReadOnlyList<TEntity>> Read { get; }
        public SelectWithParametersCacheEnty(IReadOnlyList<DbCommand> commands, Func<IReadOnlyList<DbCommand>, TParameter, IReadOnlyList<TEntity>> read) : base(commands)
        {
            Read = read;
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }

    class Cache : IDisposable
    {
        public CacheEnty? Select { get; set; }
        public CacheEnty? SelectWithParameter { get; set; }
        public CacheEnty? First { get; set; }
        public CacheEnty? FirstWithParameter { get; set; }
        public CacheEnty? Single { get; set; }
        public CacheEnty? SingleWithParameter { get; set; }

        public void Dispose()
        {
            Select?.Dispose();
        }
    }

    ConditionalWeakTable<Query<TEntity>, Cache> _caches = new();

    Cache getCache(Query<TEntity> query)
    {
        if (_caches.TryGetValue(query, out var cache))
            return cache;
        cache = new();
        _caches.Add(query, cache);
        return cache;
    }

    IReadOnlyList<ParameterValue> createParameters(Query<TEntity> query)
    {
        var parameters = new List<ParameterValue>();
        if (query.Condition != null)
            buildParameterValues(parameters, query.Condition);
        if (query.SkipCount != null)
            buildParameterValues(parameters, query.SkipCount);
        if (query.TakeCount != null)
            buildParameterValues(parameters, query.TakeCount);
        return parameters;
    }

    IReadOnlyList<DbCommand> createCommands(DbConnection connection, Query<TEntity> query, IReadOnlyList<ParameterValue>? parameters)
    {
        var meta = _metaService.GetEntityMeta<TEntity>();
        var sqls = _sqlBuilder.CreateSelectSqls(query);
        var commands = new List<DbCommand>();
        foreach (var sql in sqls)
        {
            var command = connection.CreateCommand();
            command.CommandText = sql;
            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    var p = command.CreateParameter();
                    p.ParameterName = $"@{parameter.Name}";
                    p.DbType = getDbType(parameter.Type);
                    command.Parameters.Add(p);
                }
            }
            commands.Add(command);
        }
        return commands;
    }

    public int Count(DbConnection connection, Query<TEntity> query)
    {
        throw new NotImplementedException();
    }

    public int Count<TParameter>(DbConnection connection, Query<TEntity> query, TParameter values)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<TEntity> Select(DbConnection connection, Query<TEntity> query)
    {
        var cache = getCache(query);

        IReadOnlyList<DbCommand> commands;
        Func<IReadOnlyList<DbCommand>, IReadOnlyList<TEntity>> read;
        var cacheEnty = cache.Select;
        if (cacheEnty == null)
        {
            commands = createCommands(connection, query, null);
            read = new ReadGenerator<TEntity>(_metaService).CreateReadSelectFunc(query);

            cacheEnty = new SelectCacheEnty(commands, read);
            cache.Select = cacheEnty;
        }
        else
        {
            commands = cacheEnty.Commands;
            read = ((SelectCacheEnty)cacheEnty).Read;
        }
        return read(commands);
    }

    public IReadOnlyList<TEntity> Select<TParameter>(DbConnection connection, Query<TEntity> query, TParameter values)
    {
        var cache = getCache(query);

        var parameters = createParameters(query);

        IReadOnlyList<DbCommand> commands;
        Func<IReadOnlyList<DbCommand>, TParameter, IReadOnlyList<TEntity>> read;
        var cacheEnty = cache.SelectWithParameter;
        if (cacheEnty == null)
        {
            commands = createCommands(connection, query, parameters);
            read = new ReadGenerator<TEntity>(_metaService).CreateReadSelectFunc<TParameter>(query);

            cacheEnty = new SelectWithParametersCacheEnty<TParameter>(commands, read);
            cache.SelectWithParameter = cacheEnty;
        }
        else
        {
            commands = cacheEnty.Commands;
            read = ((SelectWithParametersCacheEnty<TParameter>)cacheEnty).Read;
        }
        return read(commands, values);
    }

    public TEntity First(DbConnection connection, Query<TEntity> query)
    {
        var cache = getCache(query);

        IReadOnlyList<DbCommand> commands;
        Func<IReadOnlyList<DbCommand>, IReadOnlyList<TEntity>> read;
        var cacheEnty = cache.First;
        if (cacheEnty == null)
        {
            var firstQuery = new Query<TEntity>(query.Condition, query.Includes, query.OrderBys, query.SkipCount, new ConstantValue("1"));
            commands = createCommands(connection, firstQuery, null);
            read = new ReadGenerator<TEntity>(_metaService).CreateReadSelectFunc(firstQuery);

            cacheEnty = new SelectCacheEnty(commands, read);
            cache.First = cacheEnty;
        }
        else
        {
            commands = cacheEnty.Commands;
            read = ((SelectCacheEnty)cacheEnty).Read;
        }
        return read(commands).First();
    }

    public TEntity First<TParameter>(DbConnection connection, Query<TEntity> query, TParameter values)
    {
        var cache = getCache(query);

        var parameters = createParameters(query);

        IReadOnlyList<DbCommand> commands;
        Func<IReadOnlyList<DbCommand>, TParameter, IReadOnlyList<TEntity>> read;
        var cacheEnty = cache.FirstWithParameter;
        if (cacheEnty == null)
        {
            var firstQuery = new Query<TEntity>(query.Condition, query.Includes, query.OrderBys, query.SkipCount, new ConstantValue("1"));
            commands = createCommands(connection, firstQuery, parameters);
            read = new ReadGenerator<TEntity>(_metaService).CreateReadSelectFunc<TParameter>(firstQuery);

            cacheEnty = new SelectWithParametersCacheEnty<TParameter>(commands, read);
            cache.FirstWithParameter = cacheEnty;
        }
        else
        {
            commands = cacheEnty.Commands;
            read = ((SelectWithParametersCacheEnty<TParameter>)cacheEnty).Read;
        }
        return read(commands, values).First();
    }

    public TEntity Single(DbConnection connection, Query<TEntity> query)
    {
        var cache = getCache(query);

        IReadOnlyList<DbCommand> commands;
        Func<IReadOnlyList<DbCommand>, IReadOnlyList<TEntity>> read;
        var cacheEnty = cache.Single;
        if (cacheEnty == null)
        {
            var firstQuery = new Query<TEntity>(query.Condition, query.Includes, query.OrderBys, query.SkipCount, new ConstantValue("2"));
            commands = createCommands(connection, firstQuery, null);
            read = new ReadGenerator<TEntity>(_metaService).CreateReadSelectFunc(firstQuery);

            cacheEnty = new SelectCacheEnty(commands, read);
            cache.Single = cacheEnty;
        }
        else
        {
            commands = cacheEnty.Commands;
            read = ((SelectCacheEnty)cacheEnty).Read;
        }
        return read(commands).Single();
    }

    public TEntity Single<TParameter>(DbConnection connection, Query<TEntity> query, TParameter values)
    {
        var cache = getCache(query);

        var parameters = createParameters(query);

        IReadOnlyList<DbCommand> commands;
        Func<IReadOnlyList<DbCommand>, TParameter, IReadOnlyList<TEntity>> read;
        var cacheEnty = cache.SingleWithParameter;
        if (cacheEnty == null)
        {
            var firstQuery = new Query<TEntity>(query.Condition, query.Includes, query.OrderBys, query.SkipCount, new ConstantValue("2"));
            commands = createCommands(connection, firstQuery, parameters);
            read = new ReadGenerator<TEntity>(_metaService).CreateReadSelectFunc<TParameter>(firstQuery);

            cacheEnty = new SelectWithParametersCacheEnty<TParameter>(commands, read);
            cache.SingleWithParameter = cacheEnty;
        }
        else
        {
            commands = cacheEnty.Commands;
            read = ((SelectWithParametersCacheEnty<TParameter>)cacheEnty).Read;
        }
        return read(commands, values).Single();
    }


    static void buildParameterValues(List<ParameterValue> list, IValue src)
    {
        switch (src)
        {
            case ParameterValue v0:
                list.Add(v0);
                break;
            case IBinaryOperator v1:
                buildParameterValues(list, v1.Lhs);
                buildParameterValues(list, v1.Rhs);
                break;
        }
    }


    static DbType getDbType(Type type)
    {
        if (type == typeof(int))
            return DbType.Int32;
        else if (type == typeof(long))
            return DbType.Int64;
        else if (type == typeof(bool))
            return DbType.Boolean;
        else if (type == typeof(string))
            return DbType.String;
        else if (type == typeof(DateTime))
            return DbType.DateTime;
        else
            throw new NotSupportedException();
    }
}
