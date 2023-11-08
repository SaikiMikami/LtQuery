namespace LtQueryBenchmarks;

interface IBenchmark
{
    void Setup();
    void Cleanup();
    int Raw();
    int LtQuery();
    int Dapper();
    int EFCore();
}
