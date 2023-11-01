namespace LtQueryBenchmarks;

interface IBenchmark
{
    void Setup();
    void Cleanup();
    int LtQuery();
    int Dapper();
    //int SqlKata();
    int EFCore();
    int Raw();
}
