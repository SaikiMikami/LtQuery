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
        void Reflect(LtConnection connection);
    }
    class Difference<TEntity> : IDifference where TEntity : class
    {
        public List<TEntity> Adding { get; } = new();
        public List<TEntity> Updating { get; } = new();
        public List<TEntity> Removing { get; } = new();

        public void Reflect(LtConnection connection)
        {
            if (Adding.Count != 0)
            {
                connection.AddRange(Adding);
                Adding.Clear();
            }
            if (Updating.Count != 0)
            {
                connection.UpdateRange(Updating);
                Updating.Clear();
            }
            if (Removing.Count != 0)
            {
                connection.RemoveRange(Removing);
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
        using (var transaction = _connection.BeginTransaction(isolationLevel ?? IsolationLevel.Unspecified))
        {
            try
            {
                foreach (var meta in _metaService.AllEntityMetas)
                {
                    if (_differences.TryGetValue(meta.Type, out var value))
                    {
                        value.Reflect(_connection);
                    }
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }

    public async ValueTask CommitAsync(IsolationLevel? isolationLevel = default, CancellationToken cancellationToken = default)
    {

        var transaction = await _connection.BeginTransactionAsync(isolationLevel ?? IsolationLevel.Unspecified, cancellationToken);
        try
        {
            foreach (var meta in _metaService.AllEntityMetas)
            {
                if (_differences.TryGetValue(meta.Type, out var value))
                {
                    value.Reflect(_connection);
                }
            }
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            await transaction.DisposeAsync();
        }
    }
}
