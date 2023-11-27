namespace LtQuery.Relational;

public interface ISqlBuilder
{
    string CreateCountSql<TEntity>(Query<TEntity> query) where TEntity : class;
    string CreateSelectSql<TEntity>(Query<TEntity> query) where TEntity : class;
    //IReadOnlyList<string> CreateFirstSql<TEntity>(Query<TEntity> query) where TEntity : class;
    //IReadOnlyList<string> CreateSingleSql<TEntity>(Query<TEntity> query) where TEntity : class;

    string CreateAddSql<TEntity>() where TEntity : class;
    string CreateUpdatedSql<TEntity>() where TEntity : class;
    string CreateRemoveSql<TEntity>() where TEntity : class;
}
