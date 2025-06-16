using LtQuery.Metadata;
using Microsoft.Extensions.DependencyInjection;
using System.Collections;
using System.Data;
using System.Data.Common;

namespace LtQuery.Relational;

class LtConnection : ILtConnection
{
    readonly DbConnectionPool _pool;
    readonly EntityMetaService _metaService;
    readonly IServiceProvider _provider;
    public DbConnection Inner { get; }
    public LtConnection(DbConnectionPool pool, EntityMetaService metaService, IServiceProvider provider)
    {
        _pool = pool;
        _metaService = metaService;
        _provider = provider;
        Inner = pool.Create();
    }
    public void Dispose()
    {
        _pool.Release(Inner);
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

    public ValueTask<IReadOnlyList<TEntity>> SelectAsync<TEntity>(Query<TEntity> query, CancellationToken cancellationToken = default) where TEntity : class => GetRepository<TEntity>().SelectAsync(this, query, cancellationToken);

    public IReadOnlyList<TEntity> Select<TEntity, TParameter>(Query<TEntity> query, TParameter values) where TEntity : class => GetRepository<TEntity>().Select(this, query, values);

    public ValueTask<IReadOnlyList<TEntity>> SelectAsync<TEntity, TParameter>(Query<TEntity> query, TParameter values, CancellationToken cancellationToken = default) where TEntity : class => GetRepository<TEntity>().SelectAsync(this, query, values, cancellationToken);

    public TEntity Single<TEntity>(Query<TEntity> query) where TEntity : class => GetRepository<TEntity>().Single(this, query);

    public ValueTask<TEntity> SingleAsync<TEntity>(Query<TEntity> query, CancellationToken cancellationToken = default) where TEntity : class => GetRepository<TEntity>().SingleAsync(this, query, cancellationToken);

    public TEntity Single<TEntity, TParameter>(Query<TEntity> query, TParameter values) where TEntity : class => GetRepository<TEntity>().Single(this, query, values);

    public ValueTask<TEntity> SingleAsync<TEntity, TParameter>(Query<TEntity> query, TParameter values, CancellationToken cancellationToken = default) where TEntity : class => GetRepository<TEntity>().SingleAsync(this, query, values, cancellationToken);

    public TEntity First<TEntity>(Query<TEntity> query) where TEntity : class => GetRepository<TEntity>().First(this, query);

    public ValueTask<TEntity> FirstAsync<TEntity>(Query<TEntity> query, CancellationToken cancellationToken = default) where TEntity : class => GetRepository<TEntity>().FirstAsync(this, query, cancellationToken);

    public TEntity First<TEntity, TParameter>(Query<TEntity> query, TParameter values) where TEntity : class => GetRepository<TEntity>().First(this, query, values);

    public ValueTask<TEntity> FirstAsync<TEntity, TParameter>(Query<TEntity> query, TParameter values, CancellationToken cancellationToken = default) where TEntity : class => GetRepository<TEntity>().FirstAsync(this, query, values, cancellationToken);

    public int Count<TEntity>(Query<TEntity> query) where TEntity : class => GetRepository<TEntity>().Count(this, query);

    public ValueTask<int> CountAsync<TEntity>(Query<TEntity> query, CancellationToken cancellationToken = default) where TEntity : class => GetRepository<TEntity>().CountAsync(this, query, cancellationToken);

    public int Count<TEntity, TParameter>(Query<TEntity> query, TParameter values) where TEntity : class => GetRepository<TEntity>().Count(this, query, values);

    public ValueTask<int> CountAsync<TEntity, TParameter>(Query<TEntity> query, TParameter values, CancellationToken cancellationToken = default) where TEntity : class => GetRepository<TEntity>().CountAsync(this, query, values, cancellationToken);

    public void Add<TEntity>(TEntity entity) where TEntity : class
    {
        var type = typeof(TEntity);
        if (typeof(IEnumerable).IsAssignableFrom(type))
            throw new InvalidOperationException("when add multiple, use AddRange()");

        GetRepository<TEntity>().Add(this, new Span<TEntity>(ref entity));
    }

    public ValueTask AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class
    {
        var type = typeof(TEntity);
        if (typeof(IEnumerable).IsAssignableFrom(type))
            throw new InvalidOperationException("when add multiple, use AddRange()");

        return GetRepository<TEntity>().AddAsync(this, new TEntity[] { entity }, cancellationToken);
    }

    public void AddRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class => GetRepository<TEntity>().Add(this, entities.ToArray());

    public ValueTask AddRangeAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : class => GetRepository<TEntity>().AddAsync(this, entities.ToArray(), cancellationToken);

    public void Update<TEntity>(TEntity entity) where TEntity : class
    {
        var type = typeof(TEntity);
        if (typeof(IEnumerable).IsAssignableFrom(type))
            throw new InvalidOperationException("when update multiple, use UpdateRange()");

        GetRepository<TEntity>().Update(this, new Span<TEntity>(ref entity));
    }

    public ValueTask UpdateAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class
    {
        var type = typeof(TEntity);
        if (typeof(IEnumerable).IsAssignableFrom(type))
            throw new InvalidOperationException("when update multiple, use UpdateRange()");

        return GetRepository<TEntity>().UpdateAsync(this, new TEntity[] { entity }, cancellationToken);
    }

    public void UpdateRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class => GetRepository<TEntity>().Update(this, entities.ToArray());

    public ValueTask UpdateRangeAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : class => GetRepository<TEntity>().UpdateAsync(this, entities.ToArray(), cancellationToken);

    public void Remove<TEntity>(TEntity entity) where TEntity : class
    {
        var type = typeof(TEntity);
        if (typeof(IEnumerable).IsAssignableFrom(type))
            throw new InvalidOperationException("when remove multiple, use RemoveRange()");

        GetRepository<TEntity>().Remove(this, new Span<TEntity>(ref entity));
    }

    public ValueTask RemoveAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class
    {
        var type = typeof(TEntity);
        if (typeof(IEnumerable).IsAssignableFrom(type))
            throw new InvalidOperationException("when remove multiple, use RemoveRange()");

        return GetRepository<TEntity>().RemoveAsync(this, new TEntity[] { entity }, cancellationToken);
    }

    public void RemoveRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class => GetRepository<TEntity>().Remove(this, entities.ToArray());

    public ValueTask RemoveRangeAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : class => GetRepository<TEntity>().RemoveAsync(this, entities.ToArray(), cancellationToken);

    public ILtUnitOfWork CreateUnitOfWork() => new LtUnitOfWork(this, _metaService);


    public DbTransactionHolder? CurrentTransaction { get; private set; }

    public class DbTransactionHolder : DbTransaction
    {
        readonly LtConnection _connection;
        public DbTransaction Inner { get; }
        public DbTransactionHolder(LtConnection connection, DbTransaction transaction)
        {
            _connection = connection;
            Inner = transaction;
            connection.CurrentTransaction = this;
        }

        protected override DbConnection? DbConnection => _connection.Inner;

        public override IsolationLevel IsolationLevel => Inner.IsolationLevel;

        public override void Commit() => Inner.Commit();
        public override Task CommitAsync(CancellationToken cancellationToken = default) => Inner.CommitAsync(cancellationToken);
        public override void Rollback() => Inner.Rollback();
        public override Task RollbackAsync(CancellationToken cancellationToken = default) => Inner.RollbackAsync(cancellationToken);

        bool _disposed = false;
        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            if (disposing)
            {
                Inner.Dispose();
                _connection.CurrentTransaction = null;
            }
            _disposed = true;
        }

        public override async ValueTask DisposeAsync()
        {
            await Inner.DisposeAsync();
            _connection.CurrentTransaction = null;
            _disposed = true;
        }
    }
    public DbTransaction BeginTransaction(IsolationLevel? isolationLevel = default)
    {
        if (Inner.State == ConnectionState.Closed)
            Inner.Open();

        var transaction = Inner.BeginTransaction(isolationLevel ?? IsolationLevel.Unspecified);
        return new DbTransactionHolder(this, transaction);
    }

    public async ValueTask<DbTransaction> BeginTransactionAsync(IsolationLevel? isolationLevel = default, CancellationToken cancellationToken = default)
    {
        if (Inner.State == ConnectionState.Closed)
            await Inner.OpenAsync(cancellationToken);

        var transaction = await Inner.BeginTransactionAsync(isolationLevel ?? IsolationLevel.Unspecified, cancellationToken);
        return new DbTransactionHolder(this, transaction);
    }
}
