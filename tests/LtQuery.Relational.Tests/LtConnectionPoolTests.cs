﻿using LtQuery.TestData;
using Microsoft.Extensions.DependencyInjection;

namespace LtQuery.Relational.Tests;

public class LtConnectionPoolTests
{
    readonly IServiceProvider _provider;
    public LtConnectionPoolTests()
    {
        _provider = new ServiceProviderFactory().Create();
    }

    [Fact]
    public void ScopeTest()
    {
        ConnectionAndCommandCache cache0;
        using (var scope = _provider.CreateScope())
        {
            var provider = scope.ServiceProvider;
            var connection0 = provider.GetRequiredService<ILtConnection>();
            var connection1 = provider.GetRequiredService<ILtConnection>();
            Assert.Same(connection0, connection1);

            // Opend
            connection0.Select(Lt.Query<Blog>().Take(1));

            cache0 = ((LtConnection)connection0).ConnectionAndCommandCache;
        }
        using (var scope = _provider.CreateScope())
        {
            var provider = scope.ServiceProvider;
            var connection0 = provider.GetRequiredService<ILtConnection>();
            Assert.Same(cache0, ((LtConnection)connection0).ConnectionAndCommandCache);

            // Opend
            connection0.Select(Lt.Query<Blog>().Take(1));
        }
    }

    [Fact]
    public void ScopeTest_LostedConnection()
    {
        ConnectionAndCommandCache cache0;
        using (var scope = _provider.CreateScope())
        {
            var provider = scope.ServiceProvider;
            var connection0 = provider.GetRequiredService<ILtConnection>();

            // can Select
            connection0.Select(Lt.Query<Blog>().Take(1));

            cache0 = ((LtConnection)connection0).ConnectionAndCommandCache;

            // Dispose Connection
            cache0.Connection.Dispose();
        }
        using (var scope = _provider.CreateScope())
        {
            var provider = scope.ServiceProvider;
            var connection0 = provider.GetRequiredService<ILtConnection>();

            Assert.NotSame(cache0, ((LtConnection)connection0).ConnectionAndCommandCache);

            // can Select
            connection0.Select(Lt.Query<Blog>().Take(1));
        }
    }

    [Fact]
    public void ParallelTest()
    {
        var count = 10;
        var tasks = new Task[count];
        for (var i = 0; i < count; i++)
        {
            tasks[i] = selectAsync(_provider);
        }

        Task.WaitAll(tasks);

        tasks = new Task[count];
        for (var i = 0; i < count; i++)
        {
            tasks[i] = selectAsync(_provider);
        }

        Task.WaitAll(tasks);
    }

    Task selectAsync(IServiceProvider provider)
    {
        return Task.Run(() =>
        {
            using (var scope = provider.CreateScope())
            {
                provider = scope.ServiceProvider;
                var connection = provider.GetRequiredService<ILtConnection>();

                connection.Select(Lt.Query<Blog>().Take(100));
            }
        });
    }
}
