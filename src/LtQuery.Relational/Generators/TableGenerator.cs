using LtQuery.Metadata;
using LtQuery.Relational.Nodes;
using System.Data;
using System.Reflection.Emit;

namespace LtQuery.Relational.Generators;

class TableGenerator : AbstractGenerator
{
    public QueryGenerator QueryGenerator { get; }
    public TableNode2 Table { get; }
    public TableGenerator? Parent { get; }
    public IReadOnlyList<TableGenerator> Children { get; }
    public TableGenerator(TableGenerator? parent, QueryGenerator queryGenerator, TableNode2 table)
    {
        Parent = parent;
        QueryGenerator = queryGenerator;
        Table = table;
        var children = new List<TableGenerator>();
        foreach (var child in table.Children)
        {
            if ((child.TableType & TableType.Select) != 0)
                children.Add(new(this, queryGenerator, child));
        }
        Children = children;
    }

    bool isReuse;
    public LocalBuilder? Entity { get; private set; }
    public LocalBuilder? Id { get; private set; }
    public LocalBuilder? Dictionary { get; private set; }
    public LocalBuilder? PretId { get; private set; }


    Type dictionaryType => typeof(Dictionary<,>).MakeGenericType(Table.Key.Type, Table.Type);

    // 変数宣言
    public void CreateLocalAndLabel(ILGenerator il)
    {
        Entity = il.DeclareLocal(Type);
        if (HasSubQuery() || (Navigation != null && Navigation.NavigationType == NavigationType.Multi))
        {
            if (QueryGenerator.Parent != null)
            {
                // Dictionaryを複数に分けない。変数を再利用する。
                var sameTable = QueryGenerator.Parent.SearchTable(Table.Type);
                if (sameTable != null && sameTable.Dictionary != null)
                {
                    isReuse = true;
                    Dictionary = sameTable.Dictionary;
                }
                else
                {
                    isReuse = false;
                    Dictionary = il.DeclareLocal(dictionaryType);
                }
            }
            else
            {
                isReuse = false;
                Dictionary = il.DeclareLocal(dictionaryType);
            }
            PretId = il.DeclareLocal(Meta.Key.Type);
            Id = il.DeclareLocal(Meta.Key.Type);
        }

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
            if (!isReuse)
            {
                // var dictionary = new Dictionary<TKey, TEntity>();
                il.Emit(OpCodes.Newobj, dictionaryType.GetConstructor(Array.Empty<Type>())!);
                il.EmitStloc(Dictionary);
            }
        }
        if (Entity != null)
        {
            il.Emit(OpCodes.Ldnull);
            il.EmitStloc(Entity);
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
    public void EmitCreate(ILGenerator il, LocalBuilder reader, ref int index)
    {
        // 重複が混ざっているSELECT
        if (Navigation != null && Navigation.NavigationType == NavigationType.Multi)
        {
            if (Dictionary == null)
                throw new InvalidProgramException("Dictionary == null");
            if (Entity == null)
                throw new InvalidProgramException("Entity == null");
            if (Id == null)
                throw new InvalidProgramException("Id == null");
            if (PretId == null)
                throw new InvalidProgramException("PretId == null");

            // if(!reader.IsDBNull(0))
            var ifEnd = il.DefineLabel();
            il.EmitLdloc(1);
            il.EmitLdc_I4(index);
            il.EmitCall(DbDataReader_IsDBNull);
            il.Emit(OpCodes.Brtrue_S, ifEnd);
            {
                // 重複カット
                // id = dic[(int)reader[0]];
                emitReadColumn(il, reader, Meta.Key, index);
                il.EmitStloc(Id);

                // if(preId != id)
                var ifEnd2 = il.DefineLabel();
                il.EmitLdloc(PretId);
                il.EmitLdloc(Id);
                il.Emit(OpCodes.Beq_S, ifEnd2);
                {
                    // if(!dic.TryGetValue(id, out entity))
                    il.EmitLdloc(Dictionary);
                    il.EmitLdloc(Id);
                    il.Emit(OpCodes.Ldloca_S, Entity);
                    il.EmitCall(dictionaryType.GetMethod("TryGetValue")!);
                    il.Emit(OpCodes.Brtrue_S, ifEnd2);
                    {
                        // push key
                        il.EmitLdloc(Id);

                        var properties = Meta.Properties;
                        for (var i = 1; i < properties.Count; i++)
                        {
                            // push (?)reader[i]
                            var property = properties[i];
                            emitReadColumn(il, reader, property, index + i);
                        }

                        // push new TEntity()
                        il.Emit(OpCodes.Newobj, Type.GetConstructor(properties.Select(_ => _.Type).ToArray())!);
                        il.EmitStloc(Entity);

                        // dic.Add(id, entity)
                        il.EmitLdloc(Dictionary);
                        il.EmitLdloc(Id);
                        il.EmitLdloc(Entity);
                        il.EmitCall(dictionaryType.GetMethod("Add")!);

                    }
                }
                il.MarkLabel(ifEnd2);

                // Set Navigations
                var navigation = Navigation;
                var preEntity = Parent?.Entity;
                if (navigation != null && preEntity != null && Entity != null)
                {
                    emitSetNavigation(navigation, il, Entity, preEntity);
                    emitSetNavigation(navigation.Dest, il, preEntity, Entity);
                }
            }
            il.MarkLabel(ifEnd);
        }
        else if (Navigation != null && Navigation.NavigationType == NavigationType.Single)
        {
            // if(!reader.IsDBNull(0))
            var ifEnd = il.DefineLabel();
            il.EmitLdloc(1);
            il.EmitLdc_I4(index);
            il.EmitCall(DbDataReader_IsDBNull);
            il.Emit(OpCodes.Brtrue_S, ifEnd);
            {
                // entity = new Entity()
                emitCreate(il, reader, index);
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

                // Set Navigations
                var navigation = Navigation;
                var preEntity = Parent?.Entity;
                if (navigation != null && preEntity != null && Entity != null)
                {
                    emitSetNavigation(navigation, il, Entity, preEntity);
                    emitSetNavigation(navigation.Dest, il, preEntity, Entity);
                }
            }
            il.MarkLabel(ifEnd);
        }
        else
        {
            // サブクエリ最初のTable
            if (Parent != null && IsRootTable)
            {
                if (Parent.Dictionary == null)
                    throw new InvalidProgramException("Parent.Dictionary == null");
                if (Parent.Entity == null)
                    throw new InvalidProgramException("Parent.Entity == null");
                if (Parent.Id == null)
                    throw new InvalidProgramException("Parent.Key == null");
                if (Parent.PretId == null)
                    throw new InvalidProgramException("PreParentId == null");

                // parentId = dic[(int)reader[0]];
                emitReadColumn(il, reader, Parent.Meta.Key, index + 0);
                il.EmitStloc(Parent.Id);

                // if(preParentId != parentId)
                var ifEnd = il.DefineLabel();
                il.EmitLdloc(Parent.PretId);
                il.EmitLdloc(Parent.Id);
                il.Emit(OpCodes.Beq_S, ifEnd);
                {
                    // parentEntity = parentDictionary[parentId]
                    il.EmitLdloc(Parent.Dictionary);
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

            var hasEntities = IsRootTable && QueryGenerator.Parent == null;

            // entity = new Entity()
            emitCreate(il, reader, index);
            if (hasEntities)
            {
                if (Entity == null)
                    throw new InvalidProgramException("Entity == null");
                // entities.Add(entity)
                il.EmitLdloc(0);
                il.EmitLdloc(Entity);
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

            // Set Navigations
            var navigation = Navigation;
            var preEntity = Parent?.Entity;
            if (navigation != null && preEntity != null && Entity != null)
            {
                emitSetNavigation(navigation, il, Entity, preEntity);
                emitSetNavigation(navigation.Dest, il, preEntity, Entity);
            }
        }
        index += PropertyCount;

        foreach (var child in Children)
        {
            if ((child.Table.TableType & TableType.Select) != 0)
                child.EmitCreate(il, reader, ref index);
        }
    }

    void emitCreate(ILGenerator il, LocalBuilder reader, int index)
    {
        // push (?)reader[0]
        emitReadColumn(il, reader, Meta.Key, index + 0);
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
            emitReadColumn(il, reader, property, index + i);
        }

        // push new TEntity()
        il.Emit(OpCodes.Newobj, Type.GetConstructor(properties.Select(_ => _.Type).ToArray())!);

        if (Entity == null)
            throw new InvalidProgramException("Entity == null");
        il.EmitStloc(Entity);
    }

    static void emitReadColumn(ILGenerator il, LocalBuilder reader, PropertyMeta property, int index)
    {
        if (property.Type.IsNullable())
        {
            var type = property.Type;
            var type2 = type.GenericTypeArguments[0];
            var nullable = il.DeclareLocal(type);
            var ifEnd = il.DefineLabel();
            var elseStart = il.DefineLabel();

            // if(reader.IsDBNull(index))
            il.EmitLdloc(1);
            il.EmitLdc_I4(index);
            il.EmitCall(DbDataReader_IsDBNull);
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
                il.EmitLdloc(1);
                il.EmitLdc_I4(index);

                if (type2 == typeof(int))
                    il.EmitCall(DbDataReader_GetInt32);
                else if (type2 == typeof(long))
                    il.EmitCall(DbDataReader_GetInt64);
                else if (type2 == typeof(short))
                    il.EmitCall(DbDataReader_GetInt16);
                else if (type2 == typeof(decimal))
                    il.EmitCall(DbDataReader_GetDecimal);
                else if (type2 == typeof(byte))
                    il.EmitCall(DbDataReader_GetByte);
                else if (type2 == typeof(bool))
                    il.EmitCall(DbDataReader_GetBoolean);
                else if (type2 == typeof(Guid))
                    il.EmitCall(DbDataReader_GetGuid);
                else if (type2 == typeof(DateTime))
                    il.EmitCall(DbDataReader_GetDateTime);
                else
                    throw new NotSupportedException();
                il.Emit(OpCodes.Newobj, type.GetConstructor(new[] { type2 })!);
            }
            il.MarkLabel(ifEnd);
        }
        else if (property.Info.IsNullableReference() != false)
        {
            var type = property.Type;
            var ifEnd = il.DefineLabel();
            var elseStart = il.DefineLabel();

            // if(reader.IsDBNull(index))
            il.EmitLdloc(1);
            il.EmitLdc_I4(index);
            il.EmitCall(DbDataReader_IsDBNull);
            il.Emit(OpCodes.Brfalse_S, elseStart);
            {
                il.Emit(OpCodes.Ldnull);
                il.Emit(OpCodes.Br_S, ifEnd);
            }
            // else
            {
                il.MarkLabel(elseStart);
                il.EmitLdloc(1);
                il.EmitLdc_I4(index);

                if (type == typeof(string))
                    il.EmitCall(DbDataReader_GetString);
                else
                    throw new NotSupportedException();
            }
            il.MarkLabel(ifEnd);
        }
        else
        {
            var type = property.Type;
            il.EmitLdloc(1);
            il.EmitLdc_I4(index);
            if (type == typeof(int))
                il.EmitCall(DbDataReader_GetInt32);
            else if (type == typeof(long))
                il.EmitCall(DbDataReader_GetInt64);
            else if (type == typeof(short))
                il.EmitCall(DbDataReader_GetInt16);
            else if (type == typeof(decimal))
                il.EmitCall(DbDataReader_GetDecimal);
            else if (type == typeof(byte))
                il.EmitCall(DbDataReader_GetByte);
            else if (type == typeof(bool))
                il.EmitCall(DbDataReader_GetBoolean);
            else if (type == typeof(Guid))
                il.EmitCall(DbDataReader_GetGuid);
            else if (type == typeof(DateTime))
                il.EmitCall(DbDataReader_GetDateTime);
            else if (type == typeof(string))
                il.EmitCall(DbDataReader_GetString);
            else
                throw new NotSupportedException();
        }
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


    public TableGenerator? Search(TableNode2 table)
    {
        if (Table == table)
            return this;
        foreach (var child in Children)
        {
            var result = child.Search(table);
            if (result != null)
                return result;
        }
        return null;
    }
    public TableGenerator? Search(Type type)
    {
        if (Table.Type == type)
            return this;
        foreach (var child in Children)
        {
            var result = child.Search(type);
            if (result != null)
                return result;
        }
        return null;
    }


    public EntityMeta Meta => Table.Meta;
    public Type Type => Table.Type;
    public PropertyMeta Key => Table.Key;
    public NavigationMeta? Navigation => Table.Navigation;
    public int PropertyCount => Table.PropertyCount;
    public bool HasSubQuery() => Table.HasSubQuery();

    public bool IsRootTable => QueryGenerator.Query.RootTable == Table;
}
