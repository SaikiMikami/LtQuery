namespace LtQuery.Relational.Generators;

public interface IAddGenerator<TEntity> where TEntity : class
{
    ExecuteUpdate<TEntity> CreateExecuteAddFunc();
}
