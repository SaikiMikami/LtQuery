﻿using LtQuery.TestData;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using System.Data.Common;

namespace LtQuery.MySql.Tests;

class ServiceProviderFactory
{
    public IServiceProvider Create()
    {
        var collection = new ServiceCollection();
        collection.AddLtQueryMySql();
        collection.AddTest();
        collection.AddScoped<DbConnection>(_ => new MySqlConnection(@"server=localhost;user=ltquerytest;database=ltquerytest"));

        return collection.BuildServiceProvider();
    }
}
