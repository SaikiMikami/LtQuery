using LtQuery.Metadata;
using LtQuery.Relational.Nodes.Values;

namespace LtQuery.Relational.Nodes;

public class TableNode2
{
    public QueryNode Query { get; }
    public TableNode Node { get; }
    public TableType TableType { get; internal set; }
    public TableNode2? Parent { get; }
    public IReadOnlyList<TableNode2> Children { get; }
    public TableNode2(TableNode2? parent, QueryNode query, TableNode node, TableType tableType)
    {
        Query = query;
        Node = node;
        TableType = tableType;
        Parent = parent;
        Children = createChildren();
    }

    IReadOnlyList<TableNode2> createChildren()
    {
        var list = new List<TableNode2>();
        foreach (var child in Node.Children)
        {
            if (!child.Navigation!.IsSplited)
            {
                if ((TableType & TableType.Select) == 0)
                    list.Add(new(this, Query, child, TableType.Join));
                else
                {
                    if (HasParameterTable(child, Query.AllProperties))
                        list.Add(new(this, Query, child, TableType.Select | TableType.Join));
                    else
                        list.Add(new(this, Query, child, TableType.Select));
                }
            }
            else
            {
                if (Query.Parent == null)
                    if (HasParameterTable(child, Query.AllProperties))
                        list.Add(new(this, Query, child, TableType.Join));
            }
        }
        return list;
    }

    public static bool HasParameterTable(TableNode table, IReadOnlyList<PropertyValueData> allProperties)
    {
        if (allProperties.Any(_ => _.Table == table))
            return true;

        foreach (var child in table.Children)
        {
            if (HasParameterTable(child, allProperties))
                return true;
        }
        return false;
    }

    public bool IsJoinMany()
    {
        foreach (var child in Children)
        {
            if (child.Navigation != null)
                if (child.Navigation.Dest.NavigationType == NavigationType.Multi)
                    return true;
            if (child.IsJoinMany())
                return true;
        }
        return false;
    }

    public TableNode2? Search(TableNode table)
    {
        if (Node == table)
            return this;
        foreach (var child in Children)
        {
            var result = child.Search(table);
            if (result != null)
                return result;
        }
        return null;
    }


    public EntityMeta Meta => Node.Meta;
    public NavigationMeta? Navigation => Node.Navigation;
    public Type Type => Node.Type;
    public PropertyMeta Key => Node.Key;
    public int PropertyCount => Node.PropertyCount;
    public int Index => Node.Index;

    public bool HasSubQuery() => Node.HasSubQuery();

}
[Flags]
public enum TableType
{
    Root = 0x00,
    Select = 0x01,
    Join = 0x02,
}
