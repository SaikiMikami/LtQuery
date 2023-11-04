using System.Diagnostics;

namespace LtQueryBenchmarks;

class MyBenchmarkRunner
{
    readonly IBenchmark _benchmark;
    public MyBenchmarkRunner(IBenchmark benchmark)
    {
        _benchmark = benchmark;
    }

    class Runnable
    {
        Func<int> _func;
        Stopwatch _stopwatch;
        public Runnable(Func<int> func)
        {
            _func = func;
            _stopwatch = new Stopwatch();
        }
        public void Run()
        {
            GC.Collect();
            GC.AddMemoryPressure(10 * 1024 * 1024);
            _stopwatch.Start();
            Accum = _func();
            _stopwatch.Stop();
        }
        public int Accum { get; private set; }
        public double ElapsedMilliseconds => (double)_stopwatch.ElapsedTicks / Stopwatch.Frequency;

    }
    public void Run()
    {
        _benchmark.Setup();

        var time = new TimeSpan(10000000);

        var runnables = new List<Runnable>()
            {
                 new Runnable(_benchmark.Raw),
                 new Runnable(_benchmark.LtQuery),
                 new Runnable(_benchmark.Dapper),
                 new Runnable(_benchmark.EFCore),
            };
        var count = 0;
        var start = DateTime.Now;
        while (true)
        {
            if (DateTime.Now - start > time)
                break;
            foreach (var runnable in runnables)
                runnable.Run();
            if (count == 0)
            {
                var accums = runnables.Select(_ => _.Accum);
                if (accums.Distinct().Count() != 1)
                    throw new Exception("accums are not match");
            }
            count++;
        }

        _benchmark.Cleanup();

        using (var writer = new StreamWriter($"{_benchmark.GetType()}.txt"))
        {
            writer.WriteLine("Item, Time[ms]");
            //foreach (var runnable in runnables)
            //    writer.WriteLine($"{runnable}, {runnable.ElapsedMilliseconds / count}");

            writer.WriteLine($"LtQuery, {runnables[0].ElapsedMilliseconds / count}");
            writer.WriteLine($"Dapper, {runnables[1].ElapsedMilliseconds / count}");
            writer.WriteLine($"EFCore, {runnables[2].ElapsedMilliseconds / count}");
        }
    }
}
