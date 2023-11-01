using BenchmarkDotNet.Attributes;
using LtQueryBenchmarks.Dapper;
using LtQueryBenchmarks.EFCore;
using LtQueryBenchmarks.LtQuery;
using LtQueryBenchmarks.Raw;

namespace LtQueryBenchmarks.Benchmarks;

[Config(typeof(BenchmarkConfig))]
public class SelectAllIncludeUniqueManyBenchmark : IBenchmark
{
    LtQueryBenchmark _fastORMBenchmark;
    DapperBenchmark _dapperBenchmark;
    EFCoreBenchmark _eFCoreBenchmark;
    RawBenchmark _rawBenchmark;

    [GlobalSetup]
    public void Setup()
    {
        _fastORMBenchmark = new();
        _dapperBenchmark = new();
        _eFCoreBenchmark = new();
        _rawBenchmark = new();

        _fastORMBenchmark.Setup();
        _dapperBenchmark.Setup();
        _eFCoreBenchmark.Setup();
        _rawBenchmark.Setup();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _fastORMBenchmark?.Cleanup();
        _dapperBenchmark.Cleanup();
        _eFCoreBenchmark?.Cleanup();
        _rawBenchmark?.Cleanup();
    }

    [Benchmark]
    public int LtQuery()
    {
        return _fastORMBenchmark.SelectAllIncludeUniqueMany();
    }

    [Benchmark]
    public int Dapper()
    {
        return _dapperBenchmark.SelectAllIncludeUniqueMany();
    }

    [Benchmark]
    public int EFCore()
    {
        return _eFCoreBenchmark.SelectAllIncludeUniqueMany();
    }

    [Benchmark]
    public int Raw()
    {
        return _rawBenchmark.SelectAllIncludeUniqueMany();
    }
}
