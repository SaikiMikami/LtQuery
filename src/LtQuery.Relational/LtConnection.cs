using LtQuery.Elements;
using LtQuery.Elements.Values;
using LtQuery.Metadata;
using LtQuery.Relational.Generators;
using Microsoft.Extensions.DependencyInjection;
using System.Collections;
using System.Data;
using System.Data.Common;

namespace LtQuery.Relational;

class LtConnection : ILtConnection
{
    readonly LtConnectionPool _pool;
    readonly EntityMetaService _metaService;
    readonly IServiceProvider _provider;
    public ConnectionAndCommandCache ConnectionAndCommandCache { get; }
    DbConnection Connection => ConnectionAndCommandCache.Connection;
    public LtConnection(LtConnectionPool pool, EntityMetaService metaService, IServiceProvider provider, ConnectionAndCommandCache connectionAndCommandCache)
    {
        _pool = pool;
        _metaService = metaService;
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
    public IRepository<TEntity> GetRepository<TEntity>() where TEntity : class
    {
        var repository = RepositoryCache<TEntity>.Repository;
        if (repository == null)
        {
            repository = _provider.GetRequiredService<IRepository<TEntity>>();
            RepositoryCache<TEntity>.Repository = repository;
        }
        return repository;
    }

    public IReadOnlyList<TEntity> Select<TEntity>(Query<TEntity> query) where TEntity : class => GetRepository<TEntity>().Select(this, query);

    public IReadOnlyList<TEntity> Select<TEntity, TParameter>(Query<TEntity> query, TParameter values) where TEntity : class => GetRepository<TEntity>().Select(this, query, values);

    public TEntity Single<TEntity>(Query<TEntity> query) where TEntity : class => GetRepository<TEntity>().Single(this, query);

    public TEntity Single<TEntity, TParameter>(Query<TEntity> query, TParameter values) where TEntity : class => GetRepository<TEntity>().Single(this, query, values);

    public TEntity First<TEntity>(Query<TEntity> query) where TEntity : class => GetRepository<TEntity>().First(this, query);

    public TEntity First<TEntity, TParameter>(Query<TEntity> query, TParameter values) where TEntity : class => GetRepository<TEntity>().First(this, query, values);

    public int Count<TEntity>(Query<TEntity> query) where TEntity : class => GetRepository<TEntity>().Count(this, query);

    public int Count<TEntity, TParameter>(Query<TEntity> query, TParameter values) where TEntity : class => GetRepository<TEntity>().Count(this, query, values);

    public void Add<TEntity>(TEntity entity) where TEntity : class
    {
        var type = typeof(TEntity);
        if (typeof(IEnumerable).IsAssignableFrom(type))
            throw new InvalidOperationException("when add multiple, use AddRange()");

        GetRepository<TEntity>().Add(this, new Span<TEntity>(ref entity));
    }

    public void AddRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class => GetRepository<TEntity>().Add(this, entities.ToArray());

    public void Update<TEntity>(TEntity entity) where TEntity : class
    {
        var type = typeof(TEntity);
        if (typeof(IEnumerable).IsAssignableFrom(type))
            throw new InvalidOperationException("when update multiple, use UpdateRange()");

        GetRepository<TEntity>().Update(this, new Span<TEntity>(ref entity));
    }

    public void UpdateRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class => GetRepository<TEntity>().Update(this, entities.ToArray());

    public void Remove<TEntity>(TEntity entity) where TEntity : class
    {
        var type = typeof(TEntity);
        if (typeof(IEnumerable).IsAssignableFrom(type))
            throw new InvalidOperationException("when remove multiple, use RemoveRange()");

        GetRepository<TEntity>().Remove(this, new Span<TEntity>(ref entity));
    }

    public void RemoveRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class => GetRepository<TEntity>().Remove(this, entities.ToArray());

    public ILtUnitOfWork CreateUnitOfWork() => new LtUnitOfWork(this, _metaService);


    internal DbTransactionHolder? CurrentTransaction { get; private set; }

    public class DbTransactionHolder : IDbTransaction
    {
        readonly LtConnection _connection;
        public DbTransaction Inner { get; }
        public DbTransactionHolder(LtConnection connection, DbTransaction transaction)
        {
            _connection = connection;
            Inner = transaction;
            connection.CurrentTransaction = this;
        }

        public IDbConnection? Connection => Inner.Connection;
        public IsolationLevel IsolationLevel => Inner.IsolationLevel;

        public void Commit() => Inner.Commit();
        public void Rollback() => Inner.Rollback();

        public void Dispose()
        {
            Inner.Dispose();
            _connection.CurrentTransaction = null;
        }
    }
    public IDbTransaction BeginTransaction(IsolationLevel? isolationLevel = default)
    {
        if (Connection.State == ConnectionState.Closed)
            Connection.Open();

        var transaction = ConnectionAndCommandCache.Connection.BeginTransaction(isolationLevel ?? IsolationLevel.Unspecified);
        return new DbTransactionHolder(this, transaction);
    }


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

    public DbCommand GetAddCommand<TEntity>(string sql, int count) where TEntity : class
    {
        return createUpdateCommand<TEntity>(sql, DbMethod.Add, count);
    }

    public DbCommand GetUpdateCommand<TEntity>(string sql, int count) where TEntity : class
    {
        return createUpdateCommand<TEntity>(sql, DbMethod.Update, count);
    }

    public DbCommand GetRemoveCommand<TEntity>(string sql, int count) where TEntity : class
    {
        return createUpdateCommand<TEntity>(sql, DbMethod.Remove, count);
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

    DbCommand createUpdateCommand<TEntity>(string sql, DbMethod dbMethod, int count) where TEntity : class
    {
        if (Connection.State == ConnectionState.Closed)
            Connection.Open();

        var meta = _metaService.GetEntityMeta<TEntity>();

        var command = Connection.CreateCommand();
        command.CommandText = sql;
        for (var i = 0; i < count; i++)
        {
            foreach (var property in meta.Properties)
            {
                switch (dbMethod)
                {
                    case DbMethod.Add:
                        if (property.IsAutoIncrement)
                            continue;
                        break;
                    case DbMethod.Remove:
                        if (!property.IsKey)
                            continue;
                        break;
                }
                var p = command.CreateParameter();
                p.ParameterName = $"@{i}_{property.Name}";
                p.DbType = getDbType(property.Type);
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
