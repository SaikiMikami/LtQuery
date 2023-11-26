using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;

namespace LtQuery.Relational;

class LtConnectionPool : IDisposable
{
    readonly IServiceProvider _provider;
    public int MaxPoolSize { get; }
    public LtConnectionPool(IServiceProvider provider, LtSettings? settings)
    {
        _provider = provider;
        MaxPoolSize = settings?.MaxConnectionPoolSize ?? 100;
    }

    readonly Queue<ConnectionAndCommandCache> _unusedConnectionAndCommandCache = new();
    readonly ReaderWriterLockSlim _locker = new();

    public void Dispose()
    {
        while (_unusedConnectionAndCommandCache.Count != 0)
            _unusedConnectionAndCommandCache.Dequeue().Dispose();
    }

    public LtConnection CreateConnection()
    {
        ConnectionAndCommandCache connectionAndCommandCache;
        _locker.EnterUpgradeableReadLock();
        try
        {
            if (_unusedConnectionAndCommandCache.Count != 0)
            {
                _locker.EnterWriteLock();
                try
                {
                    connectionAndCommandCache = _unusedConnectionAndCommandCache.Dequeue();
                }
                finally
                {
                    _locker.ExitWriteLock();
                }
            }
            else
            {
                var connection = _provider.GetRequiredService<DbConnection>();
                connectionAndCommandCache = new ConnectionAndCommandCache(connection);
            }
        }
        finally
        {
            _locker.ExitUpgradeableReadLock();
        }
        return new(this, _provider, connectionAndCommandCache);
    }

    public void Release(LtConnection connection)
    {
        _locker.EnterWriteLock();
        try
        {
            var cache = connection.ConnectionAndCommandCache;
            if (cache.Connection.ConnectionString != string.Empty && _unusedConnectionAndCommandCache.Count < MaxPoolSize)
                _unusedConnectionAndCommandCache.Enqueue(cache);
            else
                cache.Dispose();
        }
        finally
        {
            _locker.ExitWriteLock();
        }
    }
}
