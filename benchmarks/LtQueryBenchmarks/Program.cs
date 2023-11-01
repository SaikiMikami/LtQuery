using BenchmarkDotNet.Running;
using LtQuery;
using LtQueryBenchmarks.Benchmarks;

namespace LtQueryBenchmarks;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            Console.WriteLine("1:checkAccums, 2:runBenchmarkDotNet, 3:myRunBenchmarks, 4:initBenchmark");
            Console.Write(':');
            var str = Console.ReadLine();
            switch (str)
            {
                case "1":
                    checkAccums();
                    break;
                case "2":
                    runBenchmarkDotNet();
                    break;
                case "3":
                    myRunBenchmarks();
                    break;
                case "4":
                    new InitBenchmark().Execute();
                    break;
                case "5":
                    new InitDataFactory().Create();
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    static void runBenchmarkDotNet()
    {
        var switcher = new BenchmarkSwitcher(new[]
        {
            //typeof(InitialBenchmark),
            typeof(SelectSingleBenchmark),
            typeof(SelectSimpleBenchmark),
            typeof(SelectAllIncludeUniqueManyBenchmark),
            typeof(SelectIncludeChilrenBenchmark),
            typeof(JoinBenchmark),
        });
        switcher.Run();
    }
    static void checkAccums()
    {
        IBenchmark benchmark;
        List<int> accums;

        //benchmark = new InitialBenchmark();
        //benchmark.Setup();
        //accums = new List<int>
        //{
        //    benchmark.LtQuery(),
        //    benchmark.EFCore(),
        //    benchmark.Dapper(),
        //};
        //benchmark.Cleanup();
        //if (accums.Distinct().Count() != 1)
        //    throw new Exception($"{benchmark.GetType().Name}: Not match accums");

        //benchmark = new SelectOneBenchmark();
        //benchmark.Setup();
        //accums = new List<int>
        //{
        //    benchmark.LtQuery(),
        //    benchmark.EFCore(),
        //    benchmark.Dapper(),
        //};
        //benchmark.Cleanup();
        //if (accums.Distinct().Count() != 1)
        //    throw new Exception($"{benchmark.GetType().Name}: Not match accums");

        //benchmark = new SelectAllBenchmark();
        //benchmark.Setup();
        //accums = new List<int>
        //{
        //    benchmark.LtQuery(),
        //    benchmark.EFCore(),
        //    benchmark.Dapper(),
        //};
        //benchmark.Cleanup();
        //if (accums.Distinct().Count() != 1)
        //    throw new Exception($"{benchmark.GetType().Name}: Not match accums");

        //benchmark = new SelectAllIncludeUniqueManyBenchmark();
        //benchmark.Setup();
        //accums = new List<int>
        //{
        //    benchmark.LtQuery(),
        //    benchmark.EFCore(),
        //    benchmark.Dapper(),
        //    benchmark.Raw(),
        //};
        //benchmark.Cleanup();
        //if (accums.Distinct().Count() != 1)
        //    throw new Exception($"{benchmark.GetType().Name}: Not match accums");

        benchmark = new SelectIncludeChilrenBenchmark();
        benchmark.Setup();
        accums = new List<int>
        {
            benchmark.LtQuery(),
            benchmark.EFCore(),
            benchmark.Dapper(),
            benchmark.Raw(),
        };
        benchmark.Cleanup();
        if (accums.Distinct().Count() != 1)
            throw new Exception($"{benchmark.GetType().Name}: Not match accums");
    }
    static void myRunBenchmarks()
    {
        //new MyBenchmarkRunner(new InitialBenchmark()).Run();
        //new MyBenchmarkRunner(new SelectSingleBenchmark()).Run();
        //new MyBenchmarkRunner(new SelectAllBenchmark()).Run();
        new MyBenchmarkRunner(new SelectIncludeChilrenBenchmark()).Run();
    }
}