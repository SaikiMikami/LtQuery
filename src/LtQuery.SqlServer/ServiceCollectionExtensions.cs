﻿using LtQuery.Relational;
using Microsoft.Extensions.DependencyInjection;

namespace LtQuery.SqlServer;

public static class ServiceCollectionExtensions
{
    public static void AddLtQuerySqlServer(this IServiceCollection _this)
    {
        _this.AddLtQueryRelational();
        _this.AddSingleton<ISqlBuilder, SqlBuilder>();
    }
}
