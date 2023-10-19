using System.Linq.Expressions;

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
        var type = exp.Type;
        var name = member.Name;

        Meta.Properties.Add(new PropertyMeta(Meta, type, name, isKey));
    }


    // 1 to Unique 0..1(Child)
    public void HasOwn<TEntity2, TKey>(Expression<Func<TEntity, TKey>> foreignKeyExpression, Expression<Func<TEntity, TEntity2>> navigationExpression, Expression<Func<TEntity2, TEntity>> destNavigationExpression, bool isKey = true) where TEntity2 : class
    {
        ForeignKeyMeta foreignKey;
        {
            var exp = (MemberExpression)foreignKeyExpression.Body;
            var member = exp.Member;
            var type = exp.Type;
            var name = member.Name;
            foreignKey = new ForeignKeyMeta(Meta, type, name, isKey);
        }

        Meta.Properties.Add(foreignKey);

        NavigationMeta navigation;
        {
            var exp = (MemberExpression)navigationExpression.Body;
            var member = exp.Member;
            var type = exp.Type;
            var name = member.Name;

            navigation = new NavigationMeta(Meta, type, name, foreignKey, true, NavigationType.SingleNotNull);
            Meta.Navigations.Add(navigation);
        }
        var destMeta = Parent.EntityTypeBuilders[typeof(TEntity2)].Meta;
        NavigationMeta destNavigation;
        {
            var exp = (MemberExpression)destNavigationExpression.Body;
            var member = exp.Member;
            var type = exp.Type;
            var name = member.Name;

            destNavigation = new NavigationMeta(destMeta, type, name, foreignKey, true, NavigationType.SingleNotNull);
            destMeta.Navigations.Add(destNavigation);
        }

        foreignKey.Init(destMeta, navigation, destNavigation);
        navigation.Init(destNavigation);
        destNavigation.Init(navigation);
    }

    // 1 to Unique * (Children)
    public void HasOwn<TEntity2, TKey>(Expression<Func<TEntity, TKey>> foreignKeyExpression, Expression<Func<TEntity, TEntity2>> navigationExpression, Expression<Func<TEntity2, IList<TEntity>>> destNavigationExpression) where TEntity2 : class
    {
        ForeignKeyMeta foreignKey;
        {
            var exp = (MemberExpression)foreignKeyExpression.Body;
            var member = exp.Member;
            var type = exp.Type;
            var name = member.Name;
            foreignKey = new ForeignKeyMeta(Meta, type, name);
        }

        Meta.Properties.Add(foreignKey);

        NavigationMeta navigation;
        {
            var exp = (MemberExpression)navigationExpression.Body;
            var member = exp.Member;
            var type = exp.Type;
            var name = member.Name;

            navigation = new NavigationMeta(Meta, type, name, foreignKey, true, NavigationType.SingleNotNull);
            Meta.Navigations.Add(navigation);
        }
        var destMeta = Parent.EntityTypeBuilders[typeof(TEntity2)].Meta;
        NavigationMeta destNavigation;
        {
            var exp = (MemberExpression)destNavigationExpression.Body;
            var member = exp.Member;
            var type = exp.Type;
            var name = member.Name;

            destNavigation = new NavigationMeta(destMeta, type, name, foreignKey, true, NavigationType.Multi);
            destMeta.Navigations.Add(destNavigation);
        }

        foreignKey.Init(destMeta, navigation, destNavigation);
        navigation.Init(destNavigation);
        destNavigation.Init(navigation);
    }
    // 1 to 0..1
    public void HasReference<TEntity2, TKey>(Expression<Func<TEntity, TKey>> foreignKeyExpression, Expression<Func<TEntity, TEntity2>> navigationExpression, Expression<Func<TEntity2, TEntity>> destNavigationExpression) where TEntity2 : class
    {
        ForeignKeyMeta foreignKey;
        {
            var exp = (MemberExpression)foreignKeyExpression.Body;
            var member = exp.Member;
            var type = exp.Type;
            var name = member.Name;
            foreignKey = new ForeignKeyMeta(Meta, type, name);
        }

        Meta.Properties.Add(foreignKey);

        NavigationMeta navigation;
        {
            var exp = (MemberExpression)navigationExpression.Body;
            var member = exp.Member;
            var type = exp.Type;
            var name = member.Name;

            NavigationType navigationType;
            if (foreignKey.Type.IsNullable())
                navigationType = NavigationType.Single;
            else
                navigationType = NavigationType.SingleNotNull;

            navigation = new NavigationMeta(Meta, type, name, foreignKey, true, navigationType);
            Meta.Navigations.Add(navigation);
        }
        var destMeta = Parent.EntityTypeBuilders[typeof(TEntity2)].Meta;
        NavigationMeta destNavigation;
        {
            var exp = (MemberExpression)destNavigationExpression.Body;
            var member = exp.Member;
            var type = exp.Type;
            var name = member.Name;

            destNavigation = new NavigationMeta(destMeta, type, name, foreignKey, true, NavigationType.Single);
            destMeta.Navigations.Add(destNavigation);
        }

        foreignKey.Init(destMeta, navigation, destNavigation);
        navigation.Init(destNavigation);
        destNavigation.Init(navigation);
    }
    // 1 to *
    public void HasReference<TEntity2, TKey>(Expression<Func<TEntity, TKey>> foreignKeyExpression, Expression<Func<TEntity, TEntity2>> navigationExpression, Expression<Func<TEntity2, IList<TEntity>>> destNavigationExpression) where TEntity2 : class
    {
        ForeignKeyMeta foreignKey;
        {
            var exp = (MemberExpression)foreignKeyExpression.Body;
            var member = exp.Member;
            var type = exp.Type;
            var name = member.Name;
            foreignKey = new ForeignKeyMeta(Meta, type, name);
        }

        Meta.Properties.Add(foreignKey);

        NavigationMeta navigation;
        {
            var exp = (MemberExpression)navigationExpression.Body;
            var member = exp.Member;
            var type = exp.Type;
            var name = member.Name;

            NavigationType navigationType;
            if (foreignKey.Type.IsNullable())
                navigationType = NavigationType.Single;
            else
                navigationType = NavigationType.SingleNotNull;

            navigation = new NavigationMeta(Meta, type, name, foreignKey, true, navigationType);
            Meta.Navigations.Add(navigation);
        }
        var destMeta = Parent.EntityTypeBuilders[typeof(TEntity2)].Meta;
        NavigationMeta destNavigation;
        {
            var exp = (MemberExpression)destNavigationExpression.Body;
            var member = exp.Member;
            var type = exp.Type;
            var name = member.Name;

            destNavigation = new NavigationMeta(destMeta, type, name, foreignKey, false, NavigationType.Multi);
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
