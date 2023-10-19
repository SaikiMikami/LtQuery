namespace LtQuery.Sql.Tests;

class TestSqlBuilder : ISqlBuilder
{

    public string CreateCountSql<TEntity>(Query<TEntity> query) where TEntity : class
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<string> CreateSelectSqls<TEntity>(Query<TEntity> query) where TEntity : class
    {
        if (query.Includes.Count() > 0)
            return new string[]
            {
                $"SELECT [Id], [Title], [CategoryId], [UserId], [DateTime], [Content] FROM [Blog]",
                $"SELECT t1.[Id], t2.[Id], t2.[BlogId], t2.[UserId], t2.[DateTime], t2.[Content] FROM [Post] AS t1 INNER JOIN [Blog] AS t2 ON t1.[Id] = t2.[BlogId]",
            };
        else
            return new string[] { $"SELECT [Id], [Title], [CategoryId], [UserId], [DateTime], [Content] FROM [Blog]" };
    }

    public IReadOnlyList<string> CreateFirstSql<TEntity>(Query<TEntity> query) where TEntity : class
    {
        throw new NotImplementedException();
    }
    public IReadOnlyList<string> CreateSingleSql<TEntity>(Query<TEntity> query) where TEntity : class
    {
        throw new NotImplementedException();
    }
}
