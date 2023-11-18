using System.Linq.Expressions;

namespace LtQuery.Metadata;

public interface IEntityTypeBuilder<TEntity> where TEntity : class
{
    void HasProperty<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression, bool isKey = false);
    void HasReference<TEntity2, TKey>(Expression<Func<TEntity, TKey?>> foreignKeyExpression, Expression<Func<TEntity, TEntity2?>> navigationExpression, Expression<Func<TEntity2, TEntity?>> destNavigationExpression) where TEntity2 : class;
    void HasReference<TEntity2, TKey>(Expression<Func<TEntity, TKey?>> foreignKeyExpression, Expression<Func<TEntity, TEntity2?>> navigationExpression, Expression<Func<TEntity2, IList<TEntity>>> destNavigationExpression) where TEntity2 : class;
}
interface IEntityTypeBuilder
{
    EntityMeta Meta { get; }
    void Finish();
}
