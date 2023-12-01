using LtQuery.Metadata;
using LtQuery.Relational;
using LtQuery.Relational.Nodes;
using System.Text;

namespace LtQuery.MySql;

class SqlBuilder : ISqlBuilder
{
    readonly EntityMetaService _metaService;
    public SqlBuilder(EntityMetaService metaService)
    {
        _metaService = metaService;
    }

    public string CreateSelectSql<TEntity>(Query<TEntity> query) where TEntity : class
    {
        var strb = new StringBuilder();
        var root = Root.Create(_metaService, query);
        appendSelectSqls(strb, root.RootQuery);
        return strb.ToString();
    }

    public string CreateCountSql<TEntity>(Query<TEntity> query) where TEntity : class
    {
        var strb = new StringBuilder();
        var root = Root.Create(_metaService, query);
        strb.AppendCountSql(root.RootQuery);
        return strb.ToString();
    }

    static void appendSelectSqls(StringBuilder strb, QueryNode query)
    {
        strb.AppendSelectSql(query, null);

        foreach (var child in query.Children)
        {
            strb.Append("; ");
            appendSelectSqls(strb, child);
        }
    }

    public string CreateAddSql<TEntity>(int count) where TEntity : class
    {
        var meta = _metaService.GetEntityMeta<TEntity>();

        var sqlb = new StringBuilder();
        sqlb.Append("INSERT INTO `").Append(meta.Type.Name).Append("` (");
        var isFirst = true;
        foreach (var property in meta.Properties)
        {
            if (property.IsAutoIncrement)
                continue;
            if (!isFirst)
                sqlb.Append(", ");
            else
                isFirst = false;
            sqlb.Append('`').Append(property.Name).Append('`');
        }
        sqlb.Append(") VALUES ");

        isFirst = true;
        for (var i = 0; i < count; i++)
        {
            if (!isFirst)
                sqlb.Append(", ");
            else
                isFirst = false;

            sqlb.Append('(');

            var isFirst2 = true;
            foreach (var property in meta.Properties)
            {
                if (property.IsAutoIncrement)
                    continue;
                if (!isFirst2)
                    sqlb.Append(", ");
                else
                    isFirst2 = false;
                sqlb.Append('@').Append(i).Append('_').Append(property.Name);
            }
            sqlb.Append(')');
        }

        if (meta.Key.IsAutoIncrement)
            sqlb.Append(" RETURNING `").Append(meta.Key.Name).Append('`');

        return sqlb.ToString();
    }

    public string CreateUpdatedSql<TEntity>(int count) where TEntity : class
    {
        var meta = _metaService.GetEntityMeta<TEntity>();

        var sqlb = new StringBuilder();

        var isFirst = true;
        for (var i = 0; i < count; i++)
        {
            if (!isFirst)
                sqlb.Append("; ");
            else
                isFirst = false;

            sqlb.Append("UPDATE `").Append(meta.Name).Append("` SET ");
            var isFirst2 = true;
            foreach (var property in meta.Properties)
            {
                if (property.IsKey)
                    continue;
                if (!isFirst2)
                    sqlb.Append(", ");
                else
                    isFirst2 = false;
                sqlb.Append('`').Append(property.Name).Append("` = @").Append(i).Append('_').Append(property.Name);
            }
            sqlb.Append(" WHERE ");

            isFirst2 = true;
            foreach (var property in meta.Properties)
            {
                if (!property.IsKey)
                    continue;
                if (!isFirst2)
                    sqlb.Append(" AND ");
                else
                    isFirst2 = false;
                sqlb.Append('`').Append(property.Name).Append("` = @").Append(i).Append('_').Append(property.Name);
            }
        }
        return sqlb.ToString();
    }

    public string CreateRemoveSql<TEntity>(int count) where TEntity : class
    {
        var meta = _metaService.GetEntityMeta<TEntity>();

        var sqlb = new StringBuilder();

        var isFirst = true;
        for (var i = 0; i < count; i++)
        {
            if (!isFirst)
                sqlb.Append("; ");
            else
                isFirst = false;

            sqlb.Append("DELETE FROM `").Append(meta.Name).Append("` WHERE ");

            var isFirst2 = true;
            foreach (var property in meta.Properties)
            {
                if (!property.IsKey)
                    continue;
                if (!isFirst2)
                    sqlb.Append(" AND ");
                else
                    isFirst2 = false;
                sqlb.Append('`').Append(property.Name).Append("` = @").Append(i).Append('_').Append(property.Name);
            }
        }
        return sqlb.ToString();
    }
}

static class StringBuilderExtensions
{
    public static StringBuilder AppendSelectSql(this StringBuilder _this, QueryNode query, TableNode2? joinToTable)
    {
        _this.appendSelectClause(query, joinToTable);
        _this.appendFromAndJoinClause(query);
        _this.appendWhereClause(query);
        _this.appendOrderBys(query);
        _this.appendTakeAndSkip(query);

        return _this;
    }

    public static StringBuilder AppendCountSql(this StringBuilder _this, QueryNode query)
    {
        _this.appendSelectForCount(query);
        _this.appendFromAndJoinClause(query);
        _this.appendWhereClause(query);
        _this.appendOrderBys(query);
        _this.appendTakeAndSkip(query);

        return _this;
    }

    static StringBuilder appendSelectClause(this StringBuilder _this, QueryNode query, TableNode2? joinToTable)
    {
        var root = query.Root;
        _this.Append("SELECT ");
        if (query.IsJoinMany())
            _this.Append("DISTINCT ");

        var isFirst = true;

        if (joinToTable != null)
        {
            // サブクエリの場合joinToTableのKeyのみ返す
            var node = joinToTable.Node.Parent!;
            _this.appendProperty(node, node.Key);

            if (node != query.RootTable.Node)
            {
                node = query.RootTable.Node;
                _this.Append(", ").appendProperty(node, node.Key).Append(" AS _sort1");
            }
            return _this;
        }

        if (query.RootTable.Node.Parent != null)
        {
            var parenttable = query.RootTable.Node.Parent!;
            _this.appendProperty(parenttable, parenttable.Key);
            isFirst = false;
        }

        void action(TableNode2 table)
        {
            if ((table.TableType & TableType.Select) != 0)
            {
                foreach (var property in table.Meta.Properties)
                {
                    if (!isFirst)
                        _this.Append(", ");
                    else
                        isFirst = false;

                    _this.appendProperty(table.Node, property);
                }
            }
            foreach (var child in table.Children)
            {
                action(child);
            }
        }
        action(query.RootTable);
        return _this;
    }

    static StringBuilder appendSelectForCount(this StringBuilder _this, QueryNode query)
    {
        var table = query.RootTable.Node;
        var key = table.Key;
        _this.Append("SELECT COUNT(").appendProperty(table, key).Append(')');
        return _this;
    }

    static StringBuilder appendFromAndJoinClause(this StringBuilder _this, QueryNode query)
    {
        var isFirst = true;

        void action0(QueryNode query)
        {
            if (query.Parent != null)
                action0(query.Parent);

            if (isFirst)
            {
                switch (query.IncludeParentType)
                {
                    case IncludeParentType.Join:
                        _this.Append(" FROM ").AppendTable(query.RootTable.Node.Parent!);
                        isFirst = false;
                        break;
                    case IncludeParentType.SubQuery:
                        _this.Append(" FROM (").AppendSelectSql(query.Parent!, query.RootTable).Append(") AS t").Append(query.RootTable.Node.Parent!.Index);
                        isFirst = false;
                        break;
                }
            }
            else
            {
                _this.appendJoinClause(query.Parent!.RootTable.Node);
            }

        }
        action0(query);


        void action(TableNode2 table)
        {
            if ((table.TableType & TableType.Select) == 0 && (table.TableType & TableType.Join) == 0)
                return;

            if (isFirst)
            {
                _this.Append(" FROM ").AppendTable(table.Node);
                isFirst = false;
            }
            else
            {
                _this.appendJoinClause(table.Node);
            }
            foreach (var child in table.Children)
            {
                action(child);
            }
        }
        action(query.RootTable);
        return _this;
    }

    static StringBuilder appendWhereClause(this StringBuilder _this, QueryNode query)
    {
        if (query.IncludeParentType == IncludeParentType.SubQuery)
            return _this;
        var conditions = query.Conditions;
        if (conditions.Count == 0)
            return _this;
        _this.Append(" WHERE ");
        var isFirst = true;
        foreach (var condition in conditions)
        {
            if (!isFirst)
                _this.Append(" AND ");
            else
                isFirst = false;

            _this.AppendValue(condition);
        }
        return _this;
    }

    static StringBuilder appendOrderBys(this StringBuilder _this, QueryNode query)
    {
        if (query.IncludeParentType == IncludeParentType.SubQuery)
            return _this;
        var orderBys = query.OrderBys;
        if (orderBys.Count == 0)
            return _this;

        _this.Append(" ORDER BY ");
        var isFirst = true;
        foreach (var orderBy in orderBys)
        {
            if (!isFirst)
                _this.Append(", ");
            else
                isFirst = false;

            _this.AppendOrderBy(orderBy);
        }
        return _this;
    }

    static StringBuilder appendTakeAndSkip(this StringBuilder _this, QueryNode query)
    {
        if (query.IncludeParentType == IncludeParentType.SubQuery)
            return _this;
        var skip = query.SkipCount;
        var take = query.TakeCount;

        if (skip != null && take != null)
            return _this.Append(" LIMIT ").AppendValue(skip).Append(", ").AppendValue(take);
        else if (skip != null)
            return _this.Append(" OFFSET ").AppendValue(skip).Append(" ROWS");
        else if (take != null)
            return _this.Append(" LIMIT ").AppendValue(take);
        else
            return _this;
    }

    static StringBuilder appendJoinClause(this StringBuilder _this, TableNode table)
    {
        var parent = table.Parent ?? throw new InvalidProgramException("table.Parent == null");
        var foreignKey = table.Navigation!.ForeignKey;
        switch (foreignKey.Navigation.NavigationType)
        {
            case NavigationType.Single:
                _this.Append(" LEFT JOIN ");
                break;
            case NavigationType.SingleNotNull:
                _this.Append(" INNER JOIN ");
                break;
            default:
                throw new InvalidProgramException();
        }
        _this.AppendTable(table).Append(" ON ");
        if (foreignKey.Parent == parent.Meta)
            _this.appendProperty(parent, foreignKey).Append(" = ").appendProperty(table, table.Key);
        else
            _this.appendProperty(parent, parent.Key).Append(" = ").appendProperty(table, foreignKey);
        return _this;
    }

    static StringBuilder appendProperty(this StringBuilder _this, TableNode table, PropertyMeta property)
    {
        _this.Append('t').Append(table.Index).Append(".`").Append(property.Name).Append('`');
        return _this;
    }
}
