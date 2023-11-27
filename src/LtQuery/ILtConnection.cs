using System.Data;

namespace LtQuery;

/// <summary>
/// LtQuery Connection
/// </summary>
public interface ILtConnection : IDbSelector, IDbUpdater, IDisposable
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
    IDbTransaction BeginTransaction(IsolationLevel? isolationLevel = default);
}
