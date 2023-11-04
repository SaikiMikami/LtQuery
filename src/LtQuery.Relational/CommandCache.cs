using System.Data.Common;

namespace LtQuery.Relational;

class CommandCache : IDisposable
{
    public DbCommand? SelectCommand { get; set; }
    public DbCommand? SignleCommand { get; set; }
    public DbCommand? FirstCommand { get; set; }
    public DbCommand? CountCommand { get; set; }

    public void Dispose()
    {
        SelectCommand?.Dispose();
        SignleCommand?.Dispose();
        FirstCommand?.Dispose();
        CountCommand?.Dispose();
    }
}
