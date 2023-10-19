using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;

namespace LtQueryBenchmarks;

internal class BenchmarkConfig : ManualConfig
{
    public const int Iterations = 500;

    public BenchmarkConfig()
    {
        AddExporter(MarkdownExporter.GitHub);
        AddDiagnoser(MemoryDiagnoser.Default);

        //AddJob(Job.ShortRun);
    }
}
