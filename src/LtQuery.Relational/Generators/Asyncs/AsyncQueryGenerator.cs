using LtQuery.Relational.Nodes;
using System.Reflection.Emit;

namespace LtQuery.Relational.Generators.Asyncs;

class AsyncQueryGenerator : AbstractGenerator
{
    public QueryNode Query { get; }
    public AsyncTableGenerator RootTable { get; }
    public AsyncQueryGenerator? Parent => ParentTable?.QueryGenerator;
    public AsyncTableGenerator? ParentTable { get; }
    public IReadOnlyList<AsyncQueryGenerator> Children { get; }
    public AsyncQueryGenerator(AsyncTableGenerator? parentTable, QueryNode query)
    {
        ParentTable = parentTable;
        Query = query;
        RootTable = new AsyncTableGenerator(parentTable, this, Query.RootTable);
        var list = new List<AsyncQueryGenerator>();
        foreach (var child in query.Children)
        {
            parentTable = RootTable.Search(child.ParentTable!) ?? throw new InvalidProgramException();
            list.Add(new(parentTable, child));
        }
        Children = list;
    }

    public void EmitInit(TypeBuilder typeb, ILGenerator il, ref int index)
    {
        RootTable.EmitInit(typeb, il, ref index);
    }

    public void EmitBody(ILGenerator il, LocalBuilder reader, LocalBuilder ret)
    {
        var index = 0;
        RootTable.EmitBody(il, reader, ret, ref index);
    }

    public void GetAllQueries(List<AsyncQueryGenerator> list)
    {
        list.Add(this);
        foreach (var child in Children)
        {
            child.GetAllQueries(list);
        }
    }

    public int GetQueryCount()
    {
        var count = 1;
        foreach (var child in Children)
        {
            count += child.GetQueryCount();
        }
        return count;
    }

    public AsyncTableGenerator? SearchTable(Type type)
    {
        var table = RootTable.Search(type);
        if (table != null)
            return table;

        if (Parent != null)
        {
            table = Parent.SearchTable(type);
            if (table != null)
                return table;
        }
        return null;
    }

    public bool HasSubQuery() => Query.Children.Count != 0;
}
