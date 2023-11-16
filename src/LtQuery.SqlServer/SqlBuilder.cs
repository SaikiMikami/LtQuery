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
        if (query.SkipCount == null && query.TakeCount != null)
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

    static StringBuilder appendFromAndJoinClause(this StringBuilder _this, QueryNode query)
    {
        var isFirst = true;

        void action0(QueryNode query)
        {
            if (query.Parent != null)
                action0(query.Parent);

            switch (query.IncludeParentType)
            {
                case IncludeParentType.Join:
                    if (isFirst)
                        _this.Append(" FROM ").AppendTable(query.RootTable.Node.Parent!);
                    else
                        _this.appendJoinClause(query.Parent!.RootTable.Node);
                    isFirst = false;
                    break;
                case IncludeParentType.SubQuery:
                    _this.Append(" FROM (").AppendSelectSql(query.Parent!, query.RootTable).Append(") AS t").Append(query.Parent!.RootTable.Index);
                    isFirst = false;
                    break;
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
                var node = table.Node;
                _this.appendJoinClause(node);
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

    static StringBuilder appendJoinClause(this StringBuilder _this, TableNode table)
    {
        var parent = table.Parent;
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
        _this.Append("t").Append(table.Index).Append(".[").Append(property.Name).Append(']');
        return _this;
    }
}
