using LtQuery.Relational.Generators;

namespace LtQuery.Relational.Tests
{
    class AddGenerator<TEntity> : IAddGenerator<TEntity> where TEntity : class
    {
        public ExecuteUpdate<TEntity> CreateExecuteAddFunc()
        {
            throw new NotImplementedException();
        }
    }
}
