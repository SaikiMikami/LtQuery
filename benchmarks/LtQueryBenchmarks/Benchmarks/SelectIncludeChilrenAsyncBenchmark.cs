using BenchmarkDotNet.Attributes;
using LtQueryBenchmarks.Dapper;
using LtQueryBenchmarks.EFCore;
using LtQueryBenchmarks.LtQuery;
using LtQueryBenchmarks.Raw;

namespace LtQueryBenchmarks.Benchmarks;

[Config(typeof(BenchmarkConfig))]
public class SelectIncludeChilrenAsyncBenchmark
{
    LtQueryBenchmark _ltQueryBenchmark = default!;
    DapperBenchmark _dapperBenchmark = default!;
    EFCoreBenchmark _eFCoreBenchmark = default!;
    RawBenchmark _rawBenchmark = default!;

    [GlobalSetup]
    public void Setup()
    {
        _rawBenchmark = new();
        _ltQueryBenchmark = new();
        _dapperBenchmark = new();
        _eFCoreBenchmark = new();

        _rawBenchmark.Setup();
        _ltQueryBenchmark.Setup();
        _dapperBenchmark.Setup();
        _eFCoreBenchmark.Setup();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _rawBenchmark?.Cleanup();
        _ltQueryBenchmark?.Cleanup();
        _dapperBenchmark.Cleanup();
        _eFCoreBenchmark?.Cleanup();
    }

    [Benchmark]
    public Task<int> Raw()
    {
        return _rawBenchmark.SelectIncludeChilrenAsync();
    }

    [Benchmark]
    public Task<int> LtQuery()
    {
        return _ltQueryBenchmark.SelectIncludeChilrenAsync();
    }

    [Benchmark]
    public Task<int> Dapper()
    {
        return _dapperBenchmark.SelectIncludeChilrenAsync();
    }

    [Benchmark]
    public Task<int> EFCore()
    {
        return _eFCoreBenchmark.SelectIncludeChilrenAsync();
    }
}
