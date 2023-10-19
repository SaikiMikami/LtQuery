namespace LtQueryBenchmarks
{
    public abstract class AbstractBenchmark
    {
        protected void AddHashCode(ref int code, object value) => code = unchecked((code * 5) ^ value?.GetHashCode() ?? 0);
    }
}
