using System.Data;
using System.Data.Common;

namespace LtQuery;

/// <summary>
/// LtQuery Connection
/// </summary>
public interface ILtConnection : IDbSelector, IDbUpdater, IDbUpdaterAsync, IDisposable
{
    /// <summary>
    /// Create UnitOfWork
    /// </summary>
    /// <returns></returns>
    ILtUnitOfWork CreateUnitOfWork();

    /// <summary>
    /// Begin Transaction
    /// </summary>
    /// <param name="isolationLevel"></param>
    /// <returns></returns>
    DbTransaction BeginTransaction(IsolationLevel? isolationLevel = default);

    /// <summary>
    /// Begin Transaction
    /// </summary>
    /// <param name="isolationLevel"></param>
    /// <returns></returns>
    ValueTask<DbTransaction> BeginTransactionAsync(IsolationLevel? isolationLevel = default, CancellationToken cancellationToken = default);
}
