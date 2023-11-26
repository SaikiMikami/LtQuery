using System.Data.Common;

namespace LtQuery.Relational;

class CommandCache : IDisposable
{
    public DbCommand? Select { get; set; }
    public DbCommand? Signle { get; set; }
    public DbCommand? First { get; set; }
    public DbCommand? Count { get; set; }

    public void Dispose()
    {
        Select?.Dispose();
        Signle?.Dispose();
        First?.Dispose();
        Count?.Dispose();
    }
}
