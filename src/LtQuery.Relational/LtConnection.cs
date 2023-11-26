using LtQuery.Elements;
using LtQuery.Elements.Values;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Data.Common;

namespace LtQuery.Relational;

class LtConnection : ILtConnection
{
    readonly LtConnectionPool _pool;
    readonly IServiceProvider _provider;
    public ConnectionAndCommandCache ConnectionAndCommandCache { get; }
    DbConnection Connection => ConnectionAndCommandCache.Connection;
    public LtConnection(LtConnectionPool pool, IServiceProvider provider, ConnectionAndCommandCache connectionAndCommandCache)
    {
        _pool = pool;
        _provider = provider;
        ConnectionAndCommandCache = connectionAndCommandCache;
    }
    public void Dispose()
    {
        _pool.Release(this);
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
            repository = _provider.GetRequiredService<IRepository<TEntity>>();
            RepositoryCache<TEntity>.Repository = repository;
        }
        return repository;
    }

    public IReadOnlyList<TEntity> Select<TEntity>(Query<TEntity> query) where TEntity : class => getRepository<TEntity>().Select(this, query);

    public IReadOnlyList<TEntity> Select<TEntity, TParameter>(Query<TEntity> query, TParameter values) where TEntity : class => getRepository<TEntity>().Select(this, query, values);

    public TEntity Single<TEntity>(Query<TEntity> query) where TEntity : class => getRepository<TEntity>().Single(this, query);

    public TEntity Single<TEntity, TParameter>(Query<TEntity> query, TParameter values) where TEntity : class => getRepository<TEntity>().Single(this, query, values);

    public TEntity First<TEntity>(Query<TEntity> query) where TEntity : class => getRepository<TEntity>().First(this, query);

    public TEntity First<TEntity, TParameter>(Query<TEntity> query, TParameter values) where TEntity : class => getRepository<TEntity>().First(this, query, values);

    public int Count<TEntity>(Query<TEntity> query) where TEntity : class => getRepository<TEntity>().Count(this, query);

    public int Count<TEntity, TParameter>(Query<TEntity> query, TParameter values) where TEntity : class => getRepository<TEntity>().Count(this, query, values);



    public DbCommand GetSelectCommand<TEntity>(Query<TEntity> query, string sql) where TEntity : class
    {
        var commandCache = ConnectionAndCommandCache.GetCommandCache(query);
        var command = commandCache.Select;
        if (command == null)
        {
            var parameters = createParameters(query);
            command = createCommand<TEntity>(sql, parameters);
            commandCache.Select = command;
        }
        return command;
    }

    public DbCommand GetSingleCommand<TEntity>(Query<TEntity> query, string sql) where TEntity : class
    {
        var commandCache = ConnectionAndCommandCache.GetCommandCache(query);
        var command = commandCache.Select;
        if (command == null)
        {
            var parameters = createParameters(query);
            command = createCommand<TEntity>(sql, parameters);
            commandCache.Select = command;
        }
        return command;
    }

    public DbCommand GetFirstCommand<TEntity>(Query<TEntity> query, string sql) where TEntity : class
    {
        var commandCache = ConnectionAndCommandCache.GetCommandCache(query);
        var command = commandCache.First;
        if (command == null)
        {
            var parameters = createParameters(query);
            command = createCommand<TEntity>(sql, parameters);
            commandCache.First = command;
        }
        return command;
    }

    public DbCommand GetCountCommand<TEntity>(Query<TEntity> query, string sql) where TEntity : class
    {
        var commandCache = ConnectionAndCommandCache.GetCommandCache(query);
        var command = commandCache.Count;
        if (command == null)
        {
            var parameters = createParameters(query);
            command = createCommand<TEntity>(sql, parameters);
            commandCache.Count = command;
        }
        return command;
    }

    DbCommand createCommand<TEntity>(string sql, IReadOnlyList<ParameterValue> parameters) where TEntity : class
    {
        if (Connection.State == ConnectionState.Closed)
            Connection.Open();

        var command = Connection.CreateCommand();
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
        return command;
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
        else if (type == typeof(short) || type == typeof(short?))
            return DbType.Int16;
        else if (type == typeof(decimal) || type == typeof(decimal?))
            return DbType.Decimal;
        else if (type == typeof(byte) || type == typeof(byte?))
            return DbType.Byte;
        else if (type == typeof(bool) || type == typeof(bool?))
            return DbType.Boolean;
        else if (type == typeof(Guid) || type == typeof(Guid?))
            return DbType.Guid;
        else if (type == typeof(DateTime) || type == typeof(DateTime?))
            return DbType.DateTime;
        else if (type == typeof(string))
            return DbType.String;
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
