using LtQuery.Elements.Values;
using LtQuery.Metadata;
using LtQuery.SqlServer.Values;
using System.Text;
using QueryNode = LtQuery.Sql.Generators.QueryNode;

namespace LtQuery.SqlServer;

class SqlBuilder : Sql.ISqlBuilder
{
    readonly EntityMetaService _metaService;
    public SqlBuilder(EntityMetaService metaService)
    {
        _metaService = metaService;
    }

    public string CreateCountSql<TEntity>(Query<TEntity> query) where TEntity : class
    {
        var meta = _metaService.GetEntityMeta<TEntity>();
        return $"SELECT COUNT(*) FROM [{meta.Type.Name}]";
    }

    public IReadOnlyList<string> CreateSelectSqls<TEntity>(Query<TEntity> query) where TEntity : class
    {
        var meta = _metaService.GetEntityMeta<TEntity>();
        var node = new QueryNode(meta, query.Condition, query.Includes, query.OrderBys, query.SkipCount, query.TakeCount);
        var tableIndex = 0;
        var queryTree = new QueryTree(node, query.Condition, query.SkipCount, query.TakeCount, query.OrderBys, ref tableIndex);

        var list = new List<string>();
        createSelectSqls(list, queryTree);
        return list;
    }

    static void createSelectSqls(List<string> list, QueryTree queryTree)
    {
        list.Add(createSelectSql(queryTree));

        foreach (var child in queryTree.Children)
            createSelectSqls(list, child);
    }
    static string createSelectSql(QueryTree query)
    {
        return new StringBuilder().AppendSelectSql(query).ToString();
    }

    //public IReadOnlyList<string> CreateFirstSql<TEntity>(Query<TEntity> query) where TEntity : class
    //{
    //    var firstQuery = new Query<TEntity>(query.Condition, query.Includes, query.OrderBys, query.SkipCount, new ConstantValue("1"));
    //    return CreateSelectSqls(firstQuery);
    //}
    //public IReadOnlyList<string> CreateSingleSql<TEntity>(Query<TEntity> query) where TEntity : class
    //{
    //    var singleQuery = new Query<TEntity>(query.Condition, query.Includes, query.OrderBys, query.SkipCount, new ConstantValue("2"));
    //    return CreateSelectSqls(singleQuery);
    //}
}

static class StringBuilderExtensions
{
    public static StringBuilder AppendSelectSql(this StringBuilder _this, QueryTree query, bool isSubQuery = false)
    {
        var topTable = query.TopTable;

        _this.appendSelectClause(query, isSubQuery);
        _this.appendFromClause(topTable);
        _this.appendJoinClause(topTable);
        _this.appendWhereClause(query, isSubQuery);
        _this.appendOrderBys(query);
        _this.appendTakeAndSkip(query.Skip, query.Take);

        return _this;
    }


    static StringBuilder appendSelectClause(this StringBuilder _this, QueryTree query, bool isSubQuery)
    {
        var table = query.TopTable;
        var parentTable = table.Parent;
        _this.Append("SELECT");
        if (parentTable == null && query.Skip == null && query.Take != null)
        {
            _this.Append(" TOP (");
            query.Take.Append(_this).Append(')');
        }

        if (isSubQuery)
        {
            _this.appendProperty(table, table.Node.Key);

        }
        else
        {
            var isFirst = true;
            if (parentTable != null)
            {
                switch (query.IncludeParentType)
                {
                    case IncludeParentType.Join:
                        _this.appendProperty(parentTable, parentTable.Node.Key);
                        isFirst = false;
                        break;
                    case IncludeParentType.SubQuery:
                        _this.appendProperty(null, parentTable.Node.Key);
                        isFirst = false;
                        break;
                }
            }
            _this.appendSelectColumns(table, ref isFirst);
        }
        return _this;
    }

    static StringBuilder appendSelectColumns(this StringBuilder _this, TableTree table, ref bool isFirst)
    {
        foreach (var property in table.Node.Meta.Properties)
        {
            if (!isFirst)
                _this.Append(',');
            else
                isFirst = false;

            _this.appendProperty(table, property);
        }
        foreach (var child in table.Children)
            _this.appendSelectColumns(child, ref isFirst);

        return _this;
    }

    static StringBuilder appendFromClause(this StringBuilder _this, TableTree table)
    {
        var parent = table.Query.Parent;
        if (parent != null)
        {
            var parentTable = table.Parent ?? throw new InvalidProgramException();
            switch (table.Query.IncludeParentType)
            {
                case IncludeParentType.None:
                    break;
                case IncludeParentType.Join:
                    _this.Append(" FROM").appendTable(parentTable);
                    break;
                case IncludeParentType.SubQuery:
                    _this.Append(" FROM (").AppendSelectSql(parent, true).Append(") AS _");
                    break;
            }
        }
        else
        {
            _this.Append(" FROM").appendTable(table);
        }

        return _this;
    }
    static StringBuilder appendJoinClause(this StringBuilder _this, TableTree table)
    {
        if (table.Parent != null)
        {
            switch (table.Query.IncludeParentType)
            {
                case IncludeParentType.SubQuery:
                    _this.appendJoinParentClause(table, table.Parent, table.Node.Navigation.ForeignKey);
                    break;
                case IncludeParentType.Join:
                    _this.appendJoinClause(table.Parent, table, table.Node.Navigation.ForeignKey);
                    break;
            }
        }
        foreach (var child in table.Children)
        {
            _this.appendJoinClause(table, child, child.Node.Navigation.ForeignKey);
        }
        return _this;
    }
    static StringBuilder appendJoinParentClause(this StringBuilder _this, TableTree table, TableTree parentTable, ForeignKeyMeta foreignKey)
    {
        _this.Append(" INNER JOIN").appendTable(table).Append(" ON");
        if (foreignKey.Parent == table.Node.Meta)
            _this.appendProperty(table, foreignKey).Append(" =").appendProperty(null, parentTable.Node.Key);
        else
            _this.appendProperty(null, table.Node.Key).Append(" =").appendProperty(parentTable, foreignKey);
        return _this;
    }
    static StringBuilder appendJoinClause(this StringBuilder _this, TableTree table, TableTree childTable, ForeignKeyMeta foreignKey)
    {
        _this.Append(" INNER JOIN").appendTable(childTable).Append(" ON");
        if (foreignKey.Parent == table.Node.Meta)
            _this.appendProperty(table, foreignKey).Append(" =").appendProperty(childTable, childTable.Node.Key);
        else
            _this.appendProperty(table, table.Node.Key).Append(" =").appendProperty(childTable, foreignKey);
        return _this;
    }
    static StringBuilder appendOrderBys(this StringBuilder _this, QueryTree query)
    {
        var orderBys = query.OrderBys;
        if (query.IncludeParentType == IncludeParentType.Join)
        {
            orderBys = query.Parent.OrderBys.Union(orderBys).ToArray();
        }

        if (orderBys.Count == 0)
            return _this;

        _this.Append(" ORDER BY ");
        var isFirst = true;
        foreach (var orderBy in orderBys)
        {
            if (!isFirst)
            {
                _this.Append(',');
                isFirst = false;
            }
            orderBy.Append(_this);
        }
        return _this;
    }

    static StringBuilder appendWhereClause(this StringBuilder _this, QueryTree query, bool isSubQuery)
    {
        var conditions = new List<IBoolValueData>();
        if (query.IncludeParentType == IncludeParentType.Join)
        {
            conditions.AddRange(query.Parent!.Conditions);
        }

        conditions.AddRange(query.Conditions);
        if (conditions.Count == 0)
            return _this;
        _this.Append(" WHERE ");
        var isFirst = true;
        foreach (var condition in conditions)
        {
            if (!isFirst)
            {
                _this.Append(" AND ");
            }
            isFirst = false;

            condition.Append(_this);
        }
        return _this;
    }

    static StringBuilder appendTakeAndSkip(this StringBuilder _this, IValueData? skip, IValueData? take)
    {
        if (skip == null)
            return _this;
        if (take != null)
        {
            _this.Append(" OFFSET ");
            skip.Append(_this).Append(" ROWS FETCH NEXT ");
            take.Append(_this).Append(" ROWS ONLY");
            return _this;
        }
        else
        {
            _this.Append(" OFFSET ");
            skip.Append(_this).Append(" ROWS");
            return _this;
        }
    }

    static StringBuilder appendTable(this StringBuilder _this, TableTree table)
        => _this.Append(" [").Append(table.Node.Meta.Name).Append("] AS t").Append(table.Index);
    static StringBuilder appendProperty(this StringBuilder _this, TableTree? table, PropertyMeta property)
    {
        if (table != null)
            _this.Append(" t").Append(table.Index);
        else
            _this.Append(" _");
        _this.Append(".[").Append(property.Name).Append(']');
        return _this;
    }
    static StringBuilder appendProperty(this StringBuilder _this, PropertyMeta property)
        => _this.Append(" _.[").Append(property.Name).Append(']');
    static StringBuilder appendArgument(this StringBuilder _this, ParameterValue argument)
        => _this.Append('@').Append(argument.Name);



    public static IReadOnlyList<TableTree> RelatedTables(IValueData condition)
    {
        var set = new HashSet<TableTree>();
        relatedTables(set, condition);
        return set.ToArray();
    }
    static void relatedTables(HashSet<TableTree> set, IValueData condition)
    {
        switch (condition)
        {
            case IBinaryOperatorData v0:
                relatedTables(set, v0.Lhs);
                relatedTables(set, v0.Rhs);
                break;
            case PropertyValueData v1:
                set.Add(v1.Table);
                break;
        }
    }

    static IReadOnlyList<IBoolValueData> relatedValueData(TableTree topTable, IReadOnlyList<IBoolValueData> boolValues)
    {
        var list = new List<IBoolValueData>();
        foreach (var boolValue in boolValues)
        {
            var relatedTables = RelatedTables(boolValue);
            if (related(relatedTables, topTable))
                list.Add(boolValue);
        }
        return list;
    }
    static bool related(IReadOnlyList<TableTree> tables, TableTree table)
    {
        if (tables.Contains(table))
            return true;
        foreach (var child in table.Children)
        {
            if (related(tables, child))
                return true;
        }
        return false;
    }
}