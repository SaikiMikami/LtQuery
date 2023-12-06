using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;

namespace LtQuery.Relational;

class DbConnectionPool : IDisposable
{
    readonly IServiceProvider _provider;
    public int MaxPoolSize { get; }
    public DbConnectionPool(IServiceProvider provider, LtSettings? settings)
    {
        _provider = provider;
        MaxPoolSize = settings?.MaxConnectionPoolSize ?? 100;
    }

    readonly Queue<DbConnection> _unusedConnection = new();
    readonly ReaderWriterLockSlim _locker = new();

    public void Dispose()
    {
        while (_unusedConnection.Count != 0)
            _unusedConnection.Dequeue().Dispose();
    }

    public DbConnection Create()
    {
        _locker.EnterUpgradeableReadLock();
        try
        {
            if (_unusedConnection.Count != 0)
            {
                _locker.EnterWriteLock();
                try
                {
                    return _unusedConnection.Dequeue();
                }
                finally
                {
                    _locker.ExitWriteLock();
                }
            }
            else
            {
                return _provider.GetRequiredService<DbConnection>();
            }
        }
        finally
        {
            _locker.ExitUpgradeableReadLock();
        }
    }

    public void Release(DbConnection connection)
    {
        _locker.EnterWriteLock();
        try
        {
            if (connection.ConnectionString != string.Empty && _unusedConnection.Count < MaxPoolSize)
                _unusedConnection.Enqueue(connection);
            else
                connection.Dispose();
        }
        finally
        {
            _locker.ExitWriteLock();
        }
    }
}
