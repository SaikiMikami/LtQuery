namespace LtQuery.Relational;

public interface ISqlBuilder
{
    string CreateCountSql<TEntity>(Query<TEntity> query) where TEntity : class;
    IReadOnlyList<string> CreateSelectSqls<TEntity>(Query<TEntity> query) where TEntity : class;
    //IReadOnlyList<string> CreateFirstSql<TEntity>(Query<TEntity> query) where TEntity : class;
    //IReadOnlyList<string> CreateSingleSql<TEntity>(Query<TEntity> query) where TEntity : class;
}
