namespace LtQuery.Relational;

public interface ISqlBuilder
{
    string CreateSelectSql<TEntity>(Query<TEntity> query) where TEntity : class;
    //IReadOnlyList<string> CreateFirstSql<TEntity>(Query<TEntity> query) where TEntity : class;
    //IReadOnlyList<string> CreateSingleSql<TEntity>(Query<TEntity> query) where TEntity : class;
    string CreateCountSql<TEntity>(Query<TEntity> query) where TEntity : class;

    string CreateAddSql<TEntity>(int count) where TEntity : class;
    string CreateUpdatedSql<TEntity>(int count) where TEntity : class;
    string CreateRemoveSql<TEntity>(int count) where TEntity : class;
}
