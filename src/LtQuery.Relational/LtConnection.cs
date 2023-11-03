using LtQuery.Elements;
using LtQuery.Elements.Values;
using LtQuery.Metadata;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace LtQuery.Relational;

class LtConnection : ILtConnection
{
    readonly EntityMetaService _metaService;
    readonly ISqlBuilder _sqlBuilder;
    public DbConnection Connection { get; }
    public LtConnection(EntityMetaService metaService, ISqlBuilder sqlBuilder, DbConnection connection)
    {
        _metaService = metaService;
        _sqlBuilder = sqlBuilder;
        Connection = connection;
        Connection.Open();
    }
    public void Dispose()
    {
        Connection.Dispose();

        foreach (var commandCaches in _commandCaches)
        {
            var commands = commandCaches.Value.SelectCommands;
            if (commands != null)
                foreach (var command in commands)
                    command.Dispose();
            commands = commandCaches.Value.SignleCommands;
            if (commands != null)
                foreach (var command in commands)
                    command.Dispose();
            commands = commandCaches.Value.FirstCommands;
            if (commands != null)
                foreach (var command in commands)
                    command.Dispose();
            commands = commandCaches.Value.CountCommands;
            if (commands != null)
                foreach (var command in commands)
                    command.Dispose();
        }
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
        return new Repository<TEntity>(_metaService);
    }


    readonly ConditionalWeakTable<object, CommandCache> _commandCaches = new();
    internal CommandCache GetCommandCache<TEntity>(Query<TEntity> query) where TEntity : class
    {
        if (!_commandCaches.TryGetValue(query, out var cache))
        {
            cache = new();
            _commandCaches.Add(query, cache);
        }
        return cache;
    }


    public IReadOnlyList<TEntity> Select<TEntity>(Query<TEntity> query) where TEntity : class => getRepository<TEntity>().Select(this, query);

    public IReadOnlyList<TEntity> Select<TEntity, TParameter>(Query<TEntity> query, TParameter values) where TEntity : class => getRepository<TEntity>().Select(this, query, values);

    public TEntity Single<TEntity>(Query<TEntity> query) where TEntity : class => getRepository<TEntity>().Single(this, query);

    public TEntity Single<TEntity, TParameter>(Query<TEntity> query, TParameter values) where TEntity : class => getRepository<TEntity>().Single(this, query, values);

    public TEntity First<TEntity>(Query<TEntity> query) where TEntity : class => getRepository<TEntity>().First(this, query);

    public TEntity First<TEntity, TParameter>(Query<TEntity> query, TParameter values) where TEntity : class => getRepository<TEntity>().First(this, query, values);

    public int Count<TEntity>(Query<TEntity> query) where TEntity : class => getRepository<TEntity>().Count(this, query);

    public int Count<TEntity, TParameter>(Query<TEntity> query, TParameter values) where TEntity : class => getRepository<TEntity>().Count(this, query, values);



    public IReadOnlyList<DbCommand> GetSelectCommands<TEntity>(Query<TEntity> query) where TEntity : class
    {
        var commandCache = GetCommandCache(query);
        var commands = commandCache.SelectCommands;
        if (commands == null)
        {
            var parameters = createParameters(query);
            commands = createCommands(Connection, query, parameters);
            commandCache.SelectCommands = commands;
        }
        return commands;
    }

    public IReadOnlyList<DbCommand> GetSingleCommands<TEntity>(Query<TEntity> query) where TEntity : class
    {
        var commandCache = GetCommandCache(query);
        var commands = commandCache.SignleCommands;
        if (commands == null)
        {
            var signleQuery = new Query<TEntity>(query.Condition, query.Includes, query.OrderBys, query.SkipCount, new ConstantValue("2"));
            var parameters = createParameters(signleQuery);
            commands = createCommands(Connection, signleQuery, parameters);
            commandCache.SignleCommands = commands;
        }
        return commands;
    }

    public IReadOnlyList<DbCommand> GetFirstCommands<TEntity>(Query<TEntity> query) where TEntity : class
    {
        var commandCache = GetCommandCache(query);
        var commands = commandCache.FirstCommands;
        if (commands == null)
        {
            var firstQuery = new Query<TEntity>(query.Condition, query.Includes, query.OrderBys, query.SkipCount, new ConstantValue("1"));
            var parameters = createParameters(firstQuery);
            commands = createCommands(Connection, firstQuery, parameters);
            commandCache.FirstCommands = commands;
        }
        return commands;
    }

    public IReadOnlyList<DbCommand> GetCountCommands<TEntity>(Query<TEntity> query) where TEntity : class
    {
        var commandCache = GetCommandCache(query);
        var commands = commandCache.CountCommands;
        if (commands == null)
        {
            var parameters = createParameters(query);
            commands = createCommands(Connection, query, parameters);
            commandCache.CountCommands = commands;
        }
        return commands;
    }

    IReadOnlyList<DbCommand> createCommands<TEntity>(DbConnection connection, Query<TEntity> query, IReadOnlyList<ParameterValue> parameters) where TEntity : class
    {
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

    IReadOnlyList<ParameterValue> createParameters<TEntity>(Query<TEntity> query) where TEntity : class
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

    static DbType getDbType(Type type)
    {
        if (type == typeof(int) || type == typeof(int?))
            return DbType.Int32;
        else if (type == typeof(long) || type == typeof(long?))
            return DbType.Int64;
        else if (type == typeof(bool) || type == typeof(bool?))
            return DbType.Boolean;
        else if (type == typeof(string))
            return DbType.String;
        else if (type == typeof(DateTime) || type == typeof(DateTime?))
            return DbType.DateTime;
        else
            throw new NotSupportedException();
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
}
