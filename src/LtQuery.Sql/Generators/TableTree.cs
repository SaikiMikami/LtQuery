using LtQuery.Metadata;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Reflection.Emit;

namespace LtQuery.Sql.Generators;

class TableTree
{
    public QueryNode Node { get; }
    public QueryTree Query { get; set; }
    public TableTree? Parent { get; }
    public List<TableTree> Children { get; } = new();
    public TableTree(QueryTree query, TableTree? parent, QueryNode node)
    {
        Query = query;
        Parent = parent;
        Node = node;

        foreach (var child in node.Children)
        {
            if (!child.Navigation!.IsSplited)
            {
                var childTree = new TableTree(query, this, child);
                Children.Add(childTree);
            }
        }
    }

    public LocalBuilder? Entity { get; private set; }
    public LocalBuilder? Id { get; private set; }
    public LocalBuilder? Dictionary { get; private set; }
    public LocalBuilder? PretId { get; private set; }


    Type dictionaryType => typeof(Dictionary<,>).MakeGenericType(Node.Key.Type, Node.Type);

    // 変数宣言
    public void CreateLocalAndLabel(ILGenerator il)
    {
        if (Node.HasSubQuery() || (Node.Navigation != null && !Node.Navigation.IsUnique))
        {
            Dictionary = il.DeclareLocal(dictionaryType);
            PretId = il.DeclareLocal(Meta.Key.Type);
            Id = il.DeclareLocal(Meta.Key.Type);
        }

        if (Node.Parent != null || Dictionary != null)
            Entity = il.DeclareLocal(Type);

        foreach (var child in Children)
        {
            child.CreateLocalAndLabel(il);
        }
    }

    // readerループの外の初期化処理
    public void EmitInit(ILGenerator il)
    {
        if (Dictionary != null)
        {
            // var dictionary = new Dictionary<TKey, TEntity>();
            il.Emit(OpCodes.Newobj, dictionaryType.GetConstructor(Array.Empty<Type>())!);
            il.EmitStloc(Dictionary);
        }
        if (Parent?.PretId != null)
        {
            il.EmitLdc_I4(0);
            il.EmitStloc(Parent.PretId);
        }
        if (PretId != null)
        {
            il.EmitLdc_I4(0);
            il.EmitStloc(PretId);
        }

        foreach (var child in Children)
        {
            child.EmitInit(il);
        }
    }

    // readerからデータを取得しentityを生成する
    public void EmitCreate(ILGenerator il, ref int index)
    {
        // 重複が混ざっているSELECT
        if (Node.Navigation != null && !Node.Navigation.IsUnique)
        {
            // 重複カット
            // id = dic[(int)reader[0]];
            emitReadColumn(il, Meta.Key.Type, index);
            il.EmitStloc(Id);

            // if(preId != id)
            var ifEnd = il.DefineLabel();
            il.EmitLdloc(PretId);
            il.EmitLdloc(Id);
            il.Emit(OpCodes.Beq_S, ifEnd);
            {
                // if(!dic.TryGetValue(id, out entity))
                il.EmitLdloc(Dictionary);
                il.EmitLdloc(Id);
                il.Emit(OpCodes.Ldloca_S, Entity);
                il.EmitCall(dictionaryType.GetMethod("TryGetValue")!);
                il.Emit(OpCodes.Brtrue_S, ifEnd);
                {
                    // push key
                    il.EmitLdloc(Id);

                    var properties = Meta.Properties;
                    for (var i = 1; i < properties.Count; i++)
                    {
                        // push (?)reader[i]
                        var property = properties[i];
                        emitReadColumn(il, property.Type, index + i);
                    }

                    // push new TEntity()
                    il.Emit(OpCodes.Newobj, Type.GetConstructor(properties.Select(_ => _.Type).ToArray())!);
                    il.EmitStloc(Entity);
                }
            }
            il.MarkLabel(ifEnd);
        }
        else
        {
            // サブクエリ最初のTable
            if (Parent != null && Query.TopTable == this)
            {
                if (Parent.Entity == null)
                    throw new InvalidProgramException("Parent.Entity == null");
                if (Parent.Id == null)
                    throw new InvalidProgramException("Parent.Key == null");
                if (Parent.PretId == null)
                    throw new InvalidProgramException("PreParentId == null");

                // parentId = dic[(int)reader[0]];
                emitReadColumn(il, Parent.Meta.Key.Type, index + 0);
                il.EmitStloc(Parent.Id);

                // if(preParentId != parentId)
                var ifEnd = il.DefineLabel();
                il.EmitLdloc(Parent.PretId);
                il.EmitLdloc(Parent.Id);
                il.Emit(OpCodes.Beq_S, ifEnd);
                {
                    // parentEntity = parentDictionary[parentId]
                    il.EmitLdloc(Parent.Dictionary!);
                    il.EmitLdloc(Parent.Id);
                    il.EmitCall(dictionaryType.GetProperty("Item", new Type[] { typeof(int) })!.GetGetMethod()!);
                    il.EmitStloc(Parent.Entity);
                    // preParentId = parentId
                    il.EmitLdloc(Parent.Id);
                    il.EmitStloc(Parent.PretId);
                }
                il.MarkLabel(ifEnd);
                index++;
            }

            var hasEntities = Query.TopTable == this && Query.Parent == null;

            if (hasEntities)
            {
                il.EmitLdloc(0);
            }
            emitCreate(il, index);
            if (Entity != null)
            {
                // entity = new Entity()
                il.EmitStloc(Entity);
            }
            if (hasEntities)
            {
                if (Entity != null)
                    il.EmitLdloc(Entity);
                // entities.Add(new Entity())
                il.EmitCall(typeof(List<>).MakeGenericType(Type).GetMethod("Add")!);
            }
            if (Dictionary != null)
            {
                if (Id == null)
                    throw new InvalidProgramException("Key == null");
                if (Entity == null)
                    throw new InvalidProgramException("Entity == null");
                il.EmitLdloc(Dictionary);
                il.EmitLdloc(Id);
                il.EmitLdloc(Entity);
                il.EmitCall(dictionaryType.GetMethod("Add")!);
            }
        }
        index += PropertyCount;

        // Set Navigations
        var navigation = Node.Navigation;
        var preEntity = Parent?.Entity;
        if (navigation != null && preEntity != null && Entity != null)
        {
            emitSetNavigation(navigation, il, Entity, preEntity);
            emitSetNavigation(navigation.Dest, il, preEntity, Entity);
        }

        foreach (var child in Children)
        {
            child.EmitCreate(il, ref index);
        }
    }

    // push 1
    void emitCreate(ILGenerator il, int index)
    {
        // push (?)reader[0]
        emitReadColumn(il, Meta.Key.Type, index + 0);
        if (Id != null)
        {
            il.EmitStloc(Id);
            il.EmitLdloc(Id);
        }

        var properties = Meta.Properties;
        for (var i = 1; i < properties.Count; i++)
        {
            // push (?)reader[i]
            var property = properties[i];
            emitReadColumn(il, property.Type, index + i);
        }

        // push new TEntity()
        il.Emit(OpCodes.Newobj, Type.GetConstructor(properties.Select(_ => _.Type).ToArray())!);
    }

    static MethodInfo _IsDBNull = typeof(DbDataReader).GetMethod("IsDBNull")!;
    static MethodInfo _GetInt32 = typeof(DbDataReader).GetMethod("GetInt32")!;
    static MethodInfo _GetString = typeof(DbDataReader).GetMethod("GetString")!;
    static MethodInfo _GetDateTime = typeof(DbDataReader).GetMethod("GetDateTime")!;
    void emitReadColumn(ILGenerator il, Type type, int index)
    {
        if (type.IsNullable())
        {
            var type2 = type.GenericTypeArguments[0];
            var nullable = il.DeclareLocal(type);
            var ifEnd = il.DefineLabel();
            var elseStart = il.DefineLabel();

            // if(reader.IsDBNull)
            il.EmitLdloc(Query.Reader!);
            il.EmitLdc_I4(index);
            il.EmitCall(_IsDBNull);
            il.Emit(OpCodes.Brfalse_S, elseStart);
            {
                il.Emit(OpCodes.Ldloca_S, nullable);
                il.Emit(OpCodes.Initobj, type);
                il.Emit(OpCodes.Ldloc_S, nullable);
                il.Emit(OpCodes.Br_S, ifEnd);
            }
            // else
            {
                il.MarkLabel(elseStart);
                il.EmitLdloc(Query.Reader!);
                il.EmitLdc_I4(index);

                if (type2 == typeof(int))
                {
                    il.EmitCall(_GetInt32);
                }
                else if (type2 == typeof(DateTime))
                {
                    il.EmitCall(_GetDateTime);
                }
                else
                {
                    throw new NotSupportedException();
                }
                il.Emit(OpCodes.Newobj, type.GetConstructor(new[] { type2 })!);
            }
            il.MarkLabel(ifEnd);
        }
        else
        {
            il.EmitLdloc(Query.Reader!);
            il.EmitLdc_I4(index);
            if (type == typeof(int))
            {
                il.EmitCall(_GetInt32);
            }
            else if (type == typeof(string))
            {
                il.EmitCall(_GetString);
            }
            else if (type == typeof(DateTime))
            {
                il.EmitCall(_GetDateTime);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }


    void emitCreateNavigations(TableTree node, ILGenerator il, ref int index, LocalBuilder preEntity)
    {
        var entity = emitCreateNavigation(Node, il, index, preEntity);
        index += node.Node.PropertyCount;
        foreach (var child in node.Children)
        {
            emitCreateNavigations(child, il, ref index, entity);
        }
    }

    LocalBuilder emitCreateNavigation(QueryNode node, ILGenerator il, int index, LocalBuilder preEntity)
    {
        var entity = il.DeclareLocal(typeof(IDataReader));

        // var entity = new()
        emitCreate(il, index);  // push 1
        il.EmitStloc(entity);

        var navigation = node.Navigation!;

        emitSetNavigation(navigation, il, entity, preEntity);
        emitSetNavigation(navigation.Dest, il, preEntity, entity);

        return entity;
    }

    void emitSetNavigation(NavigationMeta? navigation, ILGenerator il, LocalBuilder entity, LocalBuilder entity2)
    {
        if (navigation == null)
            return;
        switch (navigation.NavigationType)
        {
            case NavigationType.Multi:
                // entity1.Entity2s.Add(entity2);
                il.EmitLdloc(entity);
                il.EmitCall(navigation.PropertyInfo.GetGetMethod()!);
                il.EmitLdloc(entity2);
                il.EmitCall(navigation.Type.GetMethod("Add")!);
                break;
            case NavigationType.Single:
            case NavigationType.SingleNotNull:
                // entity1.Entity2 = entity2;
                il.EmitLdloc(entity);
                il.EmitLdloc(entity2);
                il.EmitCall(navigation.Parent.Type.GetProperty(navigation.Name)!.GetSetMethod()!);
                break;
        }
    }


    public EntityMeta Meta => Node.Meta;
    public Type Type => Meta.Type;
    public int PropertyCount => Meta.Properties.Count;
}

