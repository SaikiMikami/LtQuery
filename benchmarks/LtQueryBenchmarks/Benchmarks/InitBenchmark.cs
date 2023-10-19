using Dapper;
using LtQuery;
using LtQuery.SqlServer;
using LtQuery.TestData;
using LtQueryBenchmarks.EFCore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Diagnostics;

namespace LtQueryBenchmarks.Benchmarks;

class InitBenchmark
{
    const string _sql = "SELECT TOP(@Take) [Id], [Title], [CategoryId], [UserId], [DateTime], [Content] FROM [Blog]";
    static readonly Query<Blog> _query = Lt.Query<Blog>().Take(_ => Lt.Arg<int>("Take")).ToImmutable();
    public void Execute()
    {
        {
            using (var connection = new SqlConnection(Constants.ConnectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(_sql, connection))
                {
                    var para = command.Parameters.Add("@Take", SqlDbType.Int);
                    para.Value = 1;

                    var entities = new List<Blog>();
                    using (var reader = command.ExecuteReader())
                    {
                    }
                }
            }
            var rawStopwatch = new Stopwatch();
            GC.Collect();
            GC.AddMemoryPressure(10 * 1024 * 1024);
            rawStopwatch.Start();
            // Raw
            {
                using (var connection = new SqlConnection(Constants.ConnectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(_sql, connection))
                    {

                        var para = command.Parameters.Add("@Take", SqlDbType.Int);
                        para.Value = 1;

                        var entities = new List<Blog>();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                entities.Add(new(reader.GetInt32(0), reader.GetString(1), reader.GetInt32(2), reader.GetInt32(3), reader.GetDateTime(4), reader.GetString(5)));
                            }
                        }
                        if (entities.Count != 1)
                            throw new Exception();
                    }
                }
            }
            rawStopwatch.Stop();

            var dapperStopwatch = new Stopwatch();
            GC.Collect();
            GC.AddMemoryPressure(10 * 1024 * 1024);
            dapperStopwatch.Start();
            // Dapper
            {
                using (var connection = new SqlConnection(Constants.ConnectionString))
                {
                    var entities = connection.Query<Blog>(_sql, new { Take = 1 });
                    if (entities.Count() != 1)
                        throw new Exception();
                }
            }
            dapperStopwatch.Stop();

            var ltQueryStopwatch = new Stopwatch();
            GC.Collect();
            GC.AddMemoryPressure(10 * 1024 * 1024);
            ltQueryStopwatch.Start();
            // LtQuery
            {
                var provider = create();
                using (var scope = provider.CreateScope())
                {
                    provider = scope.ServiceProvider;

                    var connection = provider.GetRequiredService<ILtConnection>();

                    var entities = connection.Select(_query, new { Take = 1 });
                    if (entities.Count != 1)
                        throw new Exception();
                }
            }
            ltQueryStopwatch.Stop();

            var efCoreStopwatch = new Stopwatch();
            GC.Collect();
            GC.AddMemoryPressure(10 * 1024 * 1024);
            efCoreStopwatch.Start();
            // EF Core
            {
                using (var context = new TestContext())
                {
                    var entities = context.Set<Blog>().Take(1).AsNoTracking().ToArray();
                    if (entities.Length != 1)
                        throw new Exception();
                }
            }
            efCoreStopwatch.Stop();

            var rawTIme = elapsedMilliseconds(rawStopwatch);
            var dapperTIme = elapsedMilliseconds(dapperStopwatch);
            var ltQueryTIme = elapsedMilliseconds(ltQueryStopwatch);
            var efCoreTIme = elapsedMilliseconds(efCoreStopwatch);

            Console.WriteLine($"RAW : {rawTIme}");
            Console.WriteLine($"Dapper : {dapperTIme}");
            Console.WriteLine($"LtQuery : {ltQueryTIme}");
            Console.WriteLine($"EF Core : {efCoreTIme}");

            Console.ReadLine();
        }
        static IServiceProvider create()
        {
            var collection = new ServiceCollection();
            collection.AddLtQuerySqlServer();

            collection.AddTest();

            return collection.BuildServiceProvider();
        }
        static double elapsedMilliseconds(Stopwatch stopwatch) => (double)stopwatch.ElapsedTicks / Stopwatch.Frequency;
    }
}
