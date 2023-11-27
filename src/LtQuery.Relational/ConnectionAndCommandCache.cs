using System.Data.Common;
using System.Runtime.CompilerServices;

namespace LtQuery.Relational;

class ConnectionAndCommandCache : IDisposable
{
    public DbConnection Connection { get; }
    public ConnectionAndCommandCache(DbConnection connection)
    {
        Connection = connection;
    }

    public void Dispose()
    {
        foreach (var commandCaches in _commandCaches)
        {
            commandCaches.Value.Dispose();
        }
        _commandCaches.Clear();
        foreach (var commandCaches in _updateCommandCaches)
        {
            commandCaches.Value.Dispose();
        }
        _updateCommandCaches.Clear();
        Connection.Dispose();
    }

    readonly ConditionalWeakTable<object, CommandCache> _commandCaches = new();
    public CommandCache GetCommandCache<TEntity>(Query<TEntity> query) where TEntity : class
    {
        if (!_commandCaches.TryGetValue(query, out var cache))
        {
            cache = new();
            _commandCaches.Add(query, cache);
        }
        return cache;
    }

    readonly Dictionary<Type, UpdateCommandCache> _updateCommandCaches = new();
    public UpdateCommandCache GetUpdateCommandCache<TEntity>() where TEntity : class
    {
        var type = typeof(TEntity);
        if (!_updateCommandCaches.TryGetValue(type, out var cache))
        {
            cache = new();
            _updateCommandCaches.Add(type, cache);
        }
        return cache;
    }
}
