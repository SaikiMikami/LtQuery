﻿using BenchmarkDotNet.Attributes;
using LtQueryBenchmarks.Dapper;
using LtQueryBenchmarks.EFCore;
using LtQueryBenchmarks.LtQuery;
using LtQueryBenchmarks.Raw;

namespace LtQueryBenchmarks.Benchmarks;

[Config(typeof(BenchmarkConfig))]
public class SelectSimpleBenchmark : IBenchmark
{
    LtQueryBenchmark _fastORMBenchmark = default!;
    DapperBenchmark _dapperBenchmark = default!;
    EFCoreBenchmark _eFCoreBenchmark = default!;
    RawBenchmark _rawBenchmark = default!;

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
    public int Raw()
    {
        return _rawBenchmark.SelectSimple();
    }

    [Benchmark]
    public int LtQuery()
    {
        return _fastORMBenchmark.SelectSimple();
    }

    [Benchmark]
    public int Dapper()
    {
        return _dapperBenchmark.SelectSimple();
    }

    [Benchmark]
    public int EFCore()
    {
        return _eFCoreBenchmark.SelectSimple();
    }
}
