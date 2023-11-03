using QueryNode = LtQuery.Relational.Generators.QueryNode;

namespace LtQuery.SqlServer;

class TableTree
{
    public QueryNode Node { get; }
    public QueryTree Query { get; set; }
    public TableTree? Parent { get; }
    public List<TableTree> Children { get; } = new();
    public int Index { get; }
    public TableTree(QueryTree query, TableTree? parent, QueryNode node, ref int index)
    {
        Query = query;
        Parent = parent;
        Node = node;
        Index = index++;

        foreach (var child in node.Children)
        {
            if (!child.Navigation!.IsSplited)
            {
                var childTree = new TableTree(query, this, child, ref index);
                Children.Add(childTree);
            }
        }
    }
}
