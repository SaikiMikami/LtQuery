﻿namespace LtQuery.Relational.Tests;

class TestSqlBuilder : ISqlBuilder
{
    public string CreateCountSql<TEntity>(Query<TEntity> query) where TEntity : class
    {
        throw new NotImplementedException();
    }

    public string CreateSelectSql<TEntity>(Query<TEntity> query) where TEntity : class
    {
        if (query.Includes.Count > 0)
            return $@"
SELECT [Id], [Title], [CategoryId], [UserId], [DateTime], [Content] FROM [Blog];
SELECT t1.[Id], t2.[Id], t2.[BlogId], t2.[UserId], t2.[DateTime], t2.[Content] FROM [Post] AS t1 INNER JOIN [Blog] AS t2 ON t1.[Id] = t2.[BlogId]";
        else
            return $"SELECT [Id], [Title], [CategoryId], [UserId], [DateTime], [Content] FROM [Blog]";
    }

    public string CreateAddSql<TEntity>(int count) where TEntity : class
    {
        throw new NotImplementedException();
    }

    public string CreateUpdatedSql<TEntity>(int count) where TEntity : class
    {
        throw new NotImplementedException();
    }

    public string CreateRemoveSql<TEntity>(int count) where TEntity : class
    {
        throw new NotImplementedException();
    }
}
