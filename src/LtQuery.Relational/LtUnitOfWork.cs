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
    public IReadOnlyList<TEntity> Select<TEntity, TParameter>(Query<TEntity> query, TParameter values) where TEntity : class => _connection.Select(query, values);
    public TEntity Single<TEntity>(Query<TEntity> query) where TEntity : class => _connection.Single(query);
    public TEntity Single<TEntity, TParameter>(Query<TEntity> query, TParameter values) where TEntity : class => _connection.Single(query, values);
    public TEntity First<TEntity>(Query<TEntity> query) where TEntity : class => _connection.First(query);
    public TEntity First<TEntity, TParameter>(Query<TEntity> query, TParameter values) where TEntity : class => _connection.First(query, values);
    public int Count<TEntity>(Query<TEntity> query) where TEntity : class => _connection.Count(query);
    public int Count<TEntity, TParameter>(Query<TEntity> query, TParameter values) where TEntity : class => _connection.Count(query, values);

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
            connection.AddRange(Adding);
            Adding.Clear();
            connection.UpdateRange(Updating);
            Updating.Clear();
            connection.RemoveRange(Removing);
            Removing.Clear();
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
}
