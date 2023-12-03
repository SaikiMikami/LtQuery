using System.Data;

namespace LtQuery;

/// <summary>
/// Unit of Work
/// </summary>
public interface ILtUnitOfWork : IDbSelector, IDbUpdater, IDisposable
{
    /// <summary>
    /// commit
    /// </summary>
    void Commit(IsolationLevel? isolationLevel = default);

    /// <summary>
    /// commit
    /// </summary>
    ValueTask CommitAsync(IsolationLevel? isolationLevel = default, CancellationToken cancellationToken = default);
}
