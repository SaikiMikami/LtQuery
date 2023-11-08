using LtQuery.Relational.Nodes.Values;

namespace LtQuery.Relational.Nodes;

public class QueryNode
{
    public Root Root { get; }
    public TableNode2 RootTable { get; }
    public IReadOnlyList<IBoolValueData> Conditions => Root.Conditions;
    public IReadOnlyList<OrderByData> OrderBys => Root.OrderBys;
    public IValueData? SkipCount => Root.SkipCount;
    public IValueData? TakeCount => Root.TakeCount;
    public IncludeParentType IncludeParentType { get; }
    public IReadOnlyList<PropertyValueData> AllProperties => Root.AllProperties;
    public int Index { get; }
    public QueryNode? Parent => ParentTable?.Query;
    public TableNode2? ParentTable { get; }
    public List<QueryNode> Children { get; }
    public QueryNode(Root root, TableNode rootTable)
    {
        Root = root;
        IncludeParentType = getIncludeParentType(Parent);
        var tableType = TableType.Select;
        if (TableNode2.HasParameterTable(rootTable, Root.AllProperties))
            tableType |= TableType.Join;
        RootTable = new TableNode2(null, this, rootTable, tableType);

        var index = 0;
        Index = index++;

        Children = new();
        var subQueryRootTables = getSubQueryRootTables(rootTable);
        foreach (var subQueryRootTable in subQueryRootTables)
        {
            var parentTable = RootTable.Search(subQueryRootTable.Parent!) ?? throw new InvalidProgramException();
            var child = new QueryNode(Root, parentTable, subQueryRootTable, ref index);
            Children.Add(child);
        }
    }
    private QueryNode(Root root, TableNode2 parentTable, TableNode rootTable, ref int index)
    {
        Root = root;
        ParentTable = parentTable;
        IncludeParentType = getIncludeParentType(Parent);
        var tableType = TableType.Select;
        if (TableNode2.HasParameterTable(rootTable, Root.AllProperties))
            tableType |= TableType.Join;
        RootTable = new TableNode2(null, this, rootTable, tableType);
        Index = index++;

        Children = new();
        var subQueryRootTables = getSubQueryRootTables(rootTable);
        foreach (var subQueryRootTable in subQueryRootTables)
        {
            parentTable = RootTable.Search(subQueryRootTable.Parent!) ?? throw new InvalidProgramException();
            var child = new QueryNode(Root, parentTable, subQueryRootTable, ref index);
            Children.Add(child);
        }
    }


    public bool IsJoinMany() => RootTable.IsJoinMany();


    static IncludeParentType getIncludeParentType(QueryNode? parent)
    {
        if (parent == null)
            return IncludeParentType.None;
        if (parent.Conditions.Count != 0 || parent.SkipCount != null || parent.TakeCount != null)
            return IncludeParentType.SubQuery;
        return IncludeParentType.Join;
    }

    IReadOnlyList<TableNode> getSubQueryRootTables(TableNode rootTable)
    {
        var list = new List<TableNode>();
        getSubQueryRootTables(list, rootTable);
        return list;
    }
    void getSubQueryRootTables(List<TableNode> list, TableNode rootTable)
    {
        foreach (var child in rootTable.Children)
        {
            if (child.Navigation!.IsSplited)
                list.Add(child);
            else
                getSubQueryRootTables(list, child);
        }
    }
}
