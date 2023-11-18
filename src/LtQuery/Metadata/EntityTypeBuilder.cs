using System.Linq.Expressions;
using System.Reflection;

namespace LtQuery.Metadata;

class EntityTypeBuilder<TEntity> : IEntityTypeBuilder<TEntity>, IEntityTypeBuilder where TEntity : class
{
    public ModelBuilder Parent { get; }
    readonly List<Action> _initActions = new();
    public EntityMeta Meta { get; } = new EntityMeta(typeof(TEntity));
    public EntityTypeBuilder(ModelBuilder parent)
    {
        Parent = parent;
    }

    public void HasProperty<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression, bool isKey = false)
    {
        var exp = (MemberExpression)propertyExpression.Body;
        var member = exp.Member;
        var propertyInfo = (PropertyInfo)member;
        var name = member.Name;

        Meta.Properties.Add(new PropertyMeta(Meta, propertyInfo, name, isKey));
    }

    // 1 to 0..1
    public void HasReference<TEntity2, TKey>(Expression<Func<TEntity, TKey?>> foreignKeyExpression, Expression<Func<TEntity, TEntity2?>> navigationExpression, Expression<Func<TEntity2, TEntity?>> destNavigationExpression) where TEntity2 : class
    {
        ForeignKeyMeta foreignKey;
        {
            var exp = (MemberExpression)foreignKeyExpression.Body;
            var member = exp.Member;
            var foreignKeyInfo = (PropertyInfo)member;
            var name = member.Name;
            foreignKey = new ForeignKeyMeta(Meta, foreignKeyInfo, name);
        }

        Meta.Properties.Add(foreignKey);

        NavigationMeta navigation;
        {
            var exp = (MemberExpression)navigationExpression.Body;
            var member = exp.Member;
            var type = exp.Type;
            var name = member.Name;
            var propertyInfo = (PropertyInfo)member;

            NavigationType navigationType;
            if (foreignKey.Type.IsNullable() || propertyInfo.IsNullableReference())
                navigationType = NavigationType.Single;
            else
                navigationType = NavigationType.SingleNotNull;

            navigation = new NavigationMeta(Meta, type, name, foreignKey, navigationType);
            Meta.Navigations.Add(navigation);
        }
        var destMeta = Parent.EntityTypeBuilders[typeof(TEntity2)].Meta;
        NavigationMeta destNavigation;
        {
            var exp = (MemberExpression)destNavigationExpression.Body;
            var member = exp.Member;
            var type = exp.Type;
            var name = member.Name;

            destNavigation = new NavigationMeta(destMeta, type, name, foreignKey, NavigationType.Single);
            destMeta.Navigations.Add(destNavigation);
        }

        foreignKey.Init(destMeta, navigation, destNavigation);
        navigation.Init(destNavigation);
        destNavigation.Init(navigation);
    }

    // 1 to *
    public void HasReference<TEntity2, TKey>(Expression<Func<TEntity, TKey?>> foreignKeyExpression, Expression<Func<TEntity, TEntity2?>> navigationExpression, Expression<Func<TEntity2, IList<TEntity>>> destNavigationExpression) where TEntity2 : class
    {
        ForeignKeyMeta foreignKey;
        {
            var exp = (MemberExpression)foreignKeyExpression.Body;
            var member = exp.Member;
            var foreignKeyInfo = (PropertyInfo)member;
            var name = member.Name;
            foreignKey = new ForeignKeyMeta(Meta, foreignKeyInfo, name);
        }

        Meta.Properties.Add(foreignKey);

        NavigationMeta navigation;
        {
            var exp = (MemberExpression)navigationExpression.Body;
            var member = exp.Member;
            var type = exp.Type;
            var name = member.Name;
            var propertyInfo = (PropertyInfo)member;

            NavigationType navigationType;
            if (foreignKey.Type.IsNullable() || propertyInfo.IsNullableReference())
                navigationType = NavigationType.Single;
            else
                navigationType = NavigationType.SingleNotNull;

            navigation = new NavigationMeta(Meta, type, name, foreignKey, navigationType);
            Meta.Navigations.Add(navigation);
        }
        var destMeta = Parent.EntityTypeBuilders[typeof(TEntity2)].Meta;
        NavigationMeta destNavigation;
        {
            var exp = (MemberExpression)destNavigationExpression.Body;
            var member = exp.Member;
            var type = exp.Type;
            var name = member.Name;

            destNavigation = new NavigationMeta(destMeta, type, name, foreignKey, NavigationType.Multi);
            destMeta.Navigations.Add(destNavigation);
        }

        foreignKey.Init(destMeta, navigation, destNavigation);
        navigation.Init(destNavigation);
        destNavigation.Init(navigation);
    }

    public void Finish()
    {
        foreach (var initAction in _initActions)
            initAction();
    }
}
