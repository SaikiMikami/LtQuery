using LtQuery.Metadata;
using LtQuery.Relational.Nodes;
using System.Text;

namespace LtQuery.SqlServer;

class SqlBuilder : Relational.ISqlBuilder
{
    readonly EntityMetaService _metaService;
    public SqlBuilder(EntityMetaService metaService)
    {
        _metaService = metaService;
    }

    public string CreateCountSql<TEntity>(Query<TEntity> query) where TEntity : class
    {
        throw new NotImplementedException();
    }

    public string CreateSelectSql<TEntity>(Query<TEntity> query) where TEntity : class
    {
        var strb = new StringBuilder();
        var root = Root.Create(_metaService, query);
        appendSelectSqls(strb, root.RootQuery);
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
}

static class StringBuilderExtensions
{
    public static StringBuilder AppendSelectSql(this StringBuilder _this, QueryNode query, TableNode2? joinToTable)
    {
        _this.appendSelectClause(query, joinToTable);
        _this.appendFromAndJoinClause(query, joinToTable != null);  // サブクエリなら必須
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
        if (query.IncludeParentType != IncludeParentType.SubQuery && root.SkipCount == null && root.TakeCount != null)
        {
            _this.Append("TOP (").AppendValue(root.TakeCount).Append(") ");
        }

        var isFirst = true;

        if (joinToTable != null)
        {
            // サブクエリの場合joinToTableのKeyのみ返す
            var node = joinToTable.Node.Parent!;
            _this.appendProperty(node, node.Key);
            return _this;
        }

        if (query.RootTable.Node.Parent != null)
        {
            var parenttable = query.RootTable.Node.Parent!;
            if (query.IncludeParentType == IncludeParentType.Join)
                _this.appendProperty(parenttable, parenttable.Key);
            else
                _this.appendProperty(null, parenttable.Key);
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

    static StringBuilder appendFromAndJoinClause(this StringBuilder _this, QueryNode query, bool isRequired)
    {
        var isFirst = true;

        switch (query.IncludeParentType)
        {
            case IncludeParentType.Join:
                _this.Append(" FROM ").AppendTable(query.RootTable.Node.Parent!);
                isFirst = false;
                break;
            case IncludeParentType.SubQuery:
                _this.Append(" FROM (").AppendSelectSql(query.Parent!, query.RootTable).Append(") AS _");
                isFirst = false;
                break;
        }

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
                var node = table.Node;
                if (query.IncludeParentType == IncludeParentType.SubQuery && table == query.RootTable)
                    _this.appendJoinParentClause(node.Parent!, node, node.Navigation!.ForeignKey);
                else
                    _this.appendJoinClause(node.Parent!, node, node.Navigation!.ForeignKey, isRequired);
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
        var conditions = query.Root.Conditions;
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
            {
                _this.Append(", ");
                isFirst = false;
            }
            _this.AppendOrderBy(orderBy);
        }
        return _this;
    }

    static StringBuilder appendTakeAndSkip(this StringBuilder _this, QueryNode query)
    {
        if (query.IncludeParentType == IncludeParentType.SubQuery)
            return _this;
        var skip = query.SkipCount;
        if (skip == null)
            return _this;
        var take = query.TakeCount;
        if (take != null)
            return _this.Append(" OFFSET ").AppendValue(skip).Append(" ROWS FETCH NEXT ").AppendValue(take).Append(" ROWS ONLY");
        else
            return _this.Append(" OFFSET ").AppendValue(skip).Append(" ROWS");
    }

    static StringBuilder appendJoinParentClause(this StringBuilder _this, TableNode parentTable, TableNode table, ForeignKeyMeta foreignKey)
    {
        _this.Append(" INNER JOIN ").AppendTable(table).Append(" ON ");
        if (foreignKey.Parent == table.Meta)
            _this.appendProperty(null, parentTable.Key).Append(" = ").appendProperty(table, foreignKey);
        else
            _this.appendProperty(null, foreignKey).Append(" = ").appendProperty(table, table.Key);
        return _this;
    }

    static StringBuilder appendJoinClause(this StringBuilder _this, TableNode table, TableNode childTable, ForeignKeyMeta foreignKey, bool isRequired)
    {
        if (isRequired)
        {
            _this.Append(" INNER JOIN ");
        }
        else
        {
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
        }
        _this.AppendTable(childTable).Append(" ON ");
        if (foreignKey.Parent == table.Meta)
            _this.appendProperty(table, foreignKey).Append(" = ").appendProperty(childTable, childTable.Key);
        else
            _this.appendProperty(table, table.Key).Append(" = ").appendProperty(childTable, foreignKey);
        return _this;
    }

    static StringBuilder appendProperty(this StringBuilder _this, TableNode? table, PropertyMeta property)
    {
        if (table != null)
            _this.Append("t").Append(table.Index);
        else
            _this.Append("_");
        _this.Append(".[").Append(property.Name).Append(']');
        return _this;
    }
}
