using System.Data.Common;

namespace LtQuery.Relational;

class UpdateCommandCache : IDisposable
{
    public DbCommand? Add { get; set; }
    public DbCommand? Update { get; set; }
    public DbCommand? Remove { get; set; }

    public void Dispose()
    {
        Add?.Dispose();
        Update?.Dispose();
        Remove?.Dispose();
    }
}
