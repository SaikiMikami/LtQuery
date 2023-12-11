using LtQuery.Metadata;
using System.Collections;
using System.Data;

namespace LtQuery.Relational;

class LtUnitOfWork : ILtUnitOfWork
{
    readonly LtConnection _connection;
    readonly EntityMetaService _metaService;
    public LtUnitOfWork(LtConnection connection, EntityMetaService metaService)
    {
        _connection = connection;
        _metaService = metaService;
    }
    public void Dispose()
    {
    }

    public IReadOnlyList<TEntity> Select<TEntity>(Query<TEntity> query) where TEntity : class => _connection.Select(query);
    public ValueTask<IReadOnlyList<TEntity>> SelectAsync<TEntity>(Query<TEntity> query, CancellationToken cancellationToken = default) where TEntity : class => _connection.SelectAsync(query, cancellationToken);

    public IReadOnlyList<TEntity> Select<TEntity, TParameter>(Query<TEntity> query, TParameter values) where TEntity : class => _connection.Select(query, values);
    public ValueTask<IReadOnlyList<TEntity>> SelectAsync<TEntity, TParameter>(Query<TEntity> query, TParameter values, CancellationToken cancellationToken = default) where TEntity : class => _connection.SelectAsync(query, values, cancellationToken);

    public TEntity Single<TEntity>(Query<TEntity> query) where TEntity : class => _connection.Single(query);
    public ValueTask<TEntity> SingleAsync<TEntity>(Query<TEntity> query, CancellationToken cancellationToken = default) where TEntity : class => _connection.SingleAsync(query);

    public TEntity Single<TEntity, TParameter>(Query<TEntity> query, TParameter values) where TEntity : class => _connection.Single(query, values);
    public ValueTask<TEntity> SingleAsync<TEntity, TParameter>(Query<TEntity> query, TParameter values, CancellationToken cancellationToken = default) where TEntity : class => _connection.SingleAsync(query, values, cancellationToken);

    public TEntity First<TEntity>(Query<TEntity> query) where TEntity : class => _connection.First(query);
    public ValueTask<TEntity> FirstAsync<TEntity>(Query<TEntity> query, CancellationToken cancellationToken = default) where TEntity : class => _connection.FirstAsync(query);

    public TEntity First<TEntity, TParameter>(Query<TEntity> query, TParameter values) where TEntity : class => _connection.First(query, values);
    public ValueTask<TEntity> FirstAsync<TEntity, TParameter>(Query<TEntity> query, TParameter values, CancellationToken cancellationToken = default) where TEntity : class => _connection.FirstAsync(query, values, cancellationToken);

    public int Count<TEntity>(Query<TEntity> query) where TEntity : class => _connection.Count(query);
    public ValueTask<int> CountAsync<TEntity>(Query<TEntity> query, CancellationToken cancellationToken = default) where TEntity : class => _connection.CountAsync(query);

    public int Count<TEntity, TParameter>(Query<TEntity> query, TParameter values) where TEntity : class => _connection.Count(query, values);
    public ValueTask<int> CountAsync<TEntity, TParameter>(Query<TEntity> query, TParameter values, CancellationToken cancellationToken = default) where TEntity : class => _connection.CountAsync(query, values, cancellationToken);

    interface IDifference
    {
        void ExecuteAdd(LtConnection connection);
        ValueTask ExecuteAddAsync(LtConnection connection, CancellationToken cancellationToken);
        void ExecuteUpdate(LtConnection connection);
        ValueTask ExecuteUpdateAsync(LtConnection connection, CancellationToken cancellationToken);
        void ExecuteRemove(LtConnection connection);
        ValueTask ExecuteRemoveAsync(LtConnection connection, CancellationToken cancellationToken);
    }
    class Difference<TEntity> : IDifference where TEntity : class
    {
        public List<TEntity> Adding { get; } = new();
        public List<TEntity> Updating { get; } = new();
        public List<TEntity> Removing { get; } = new();

        public void ExecuteAdd(LtConnection connection)
        {
            if (Adding.Count != 0)
            {
                connection.AddRange(Adding);
                Adding.Clear();
            }
        }
        public async ValueTask ExecuteAddAsync(LtConnection connection, CancellationToken cancellationToken)
        {
            if (Adding.Count != 0)
            {
                await connection.AddRangeAsync(Adding, cancellationToken);
                Adding.Clear();
            }
        }
        public void ExecuteUpdate(LtConnection connection)
        {
            if (Updating.Count != 0)
            {
                connection.UpdateRange(Updating);
                Updating.Clear();
            }
        }
        public async ValueTask ExecuteUpdateAsync(LtConnection connection, CancellationToken cancellationToken)
        {
            if (Updating.Count != 0)
            {
                await connection.UpdateRangeAsync(Updating, cancellationToken);
                Updating.Clear();
            }
        }
        public void ExecuteRemove(LtConnection connection)
        {
            if (Removing.Count != 0)
            {
                connection.RemoveRange(Removing);
                Removing.Clear();
            }
        }
        public async ValueTask ExecuteRemoveAsync(LtConnection connection, CancellationToken cancellationToken)
        {
            if (Removing.Count != 0)
            {
                await connection.RemoveRangeAsync(Removing, cancellationToken);
                Removing.Clear();
            }
        }
    }

    Dictionary<Type, IDifference> _differences { get; } = new();

    public void Add<TEntity>(TEntity entity) where TEntity : class
    {
        var type = typeof(TEntity);
        if (typeof(IEnumerable).IsAssignableFrom(type))
            throw new InvalidOperationException("when add multiple, use AddRange()");

        if (!_differences.TryGetValue(type, out var value))
        {
            value = new Difference<TEntity>();
            _differences.Add(type, value);
        }
        var value2 = (Difference<TEntity>)value;
        value2.Adding.Add(entity);
    }

    public void AddRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
    {
        var type = typeof(TEntity);
        if (!_differences.TryGetValue(type, out var value))
        {
            value = new Difference<TEntity>();
            _differences.Add(type, value);
        }
        var value2 = (Difference<TEntity>)value;
        value2.Adding.AddRange(entities);
    }

    public void Update<TEntity>(TEntity entity) where TEntity : class
    {
        var type = typeof(TEntity);
        if (typeof(IEnumerable).IsAssignableFrom(type))
            throw new InvalidOperationException("when update multiple, use UpdateRange()");

        if (!_differences.TryGetValue(type, out var value))
        {
            value = new Difference<TEntity>();
            _differences.Add(type, value);
        }
        var value2 = (Difference<TEntity>)value;
        value2.Updating.Add(entity);
    }

    public void UpdateRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
    {
        var type = typeof(TEntity);
        if (!_differences.TryGetValue(type, out var value))
        {
            value = new Difference<TEntity>();
            _differences.Add(type, value);
        }
        var value2 = (Difference<TEntity>)value;
        value2.Updating.AddRange(entities);
    }

    public void Remove<TEntity>(TEntity entity) where TEntity : class
    {
        var type = typeof(TEntity);
        if (typeof(IEnumerable).IsAssignableFrom(type))
            throw new InvalidOperationException("when remove multiple, use RemoveRange()");

        if (!_differences.TryGetValue(type, out var value))
        {
            value = new Difference<TEntity>();
            _differences.Add(type, value);
        }
        var value2 = (Difference<TEntity>)value;
        value2.Removing.Add(entity);
    }

    public void RemoveRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
    {
        var type = typeof(TEntity);
        if (!_differences.TryGetValue(type, out var value))
        {
            value = new Difference<TEntity>();
            _differences.Add(type, value);
        }
        var value2 = (Difference<TEntity>)value;
        value2.Removing.AddRange(entities);
    }

    public void Commit(IsolationLevel? isolationLevel = default)
    {
        if (_connection.CurrentTransaction == null)
            beginTransactionAndCommit(isolationLevel);
        else
            commit();
    }

    public ValueTask CommitAsync(IsolationLevel? isolationLevel = default, CancellationToken cancellationToken = default)
    {
        if (_connection.CurrentTransaction == null)
            return beginTransactionAndCommitAsync(isolationLevel, cancellationToken);
        else
            return commitAsync(cancellationToken);
    }

    void beginTransactionAndCommit(IsolationLevel? isolationLevel = default)
    {
        using (var transaction = _connection.BeginTransaction(isolationLevel ?? IsolationLevel.Unspecified))
        {
            try
            {
                commit();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }

    async ValueTask beginTransactionAndCommitAsync(IsolationLevel? isolationLevel, CancellationToken cancellationToken)
    {
        using (var transaction = await _connection.BeginTransactionAsync(isolationLevel ?? IsolationLevel.Unspecified, cancellationToken))
        {
            try
            {
                await commitAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }

    void commit()
    {
        var all = _metaService.AllEntityMetas;

        // Add
        foreach (var meta in all)
        {
            if (_differences.TryGetValue(meta.Type, out var value))
            {
                value.ExecuteAdd(_connection);
            }
        }

        // Update
        foreach (var meta in all)
        {
            if (_differences.TryGetValue(meta.Type, out var value))
            {
                value.ExecuteUpdate(_connection);
            }
        }

        // Remove
        foreach (var meta in all.Reverse())
        {
            if (_differences.TryGetValue(meta.Type, out var value))
            {
                value.ExecuteRemove(_connection);
            }
        }
    }

    async ValueTask commitAsync(CancellationToken cancellationToken)
    {
        var all = _metaService.AllEntityMetas;

        // Add
        foreach (var meta in all)
        {
            if (_differences.TryGetValue(meta.Type, out var value))
            {
                await value.ExecuteAddAsync(_connection, cancellationToken);
            }
        }

        // Update
        foreach (var meta in all)
        {
            if (_differences.TryGetValue(meta.Type, out var value))
            {
                await value.ExecuteUpdateAsync(_connection, cancellationToken);
            }
        }

        // Remove
        foreach (var meta in all.Reverse())
        {
            if (_differences.TryGetValue(meta.Type, out var value))
            {
                await value.ExecuteRemoveAsync(_connection, cancellationToken);
            }
        }
    }
}
