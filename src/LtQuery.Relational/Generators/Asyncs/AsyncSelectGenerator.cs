using LtQuery.Metadata;
using LtQuery.Relational.Nodes;
using System.Data.Common;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace LtQuery.Relational.Generators.Asyncs;

class AsyncSelectGenerator : AbstractGenerator
{
    readonly EntityMetaService _metaService;
    readonly ModuleBuilder _moduleBuilder;

    readonly TypeBuilder _typeb;
    readonly Type _returnType;
    readonly Type _returnImplementationType;
    readonly Type _returnValueTaskType;
    readonly Type _builderType;

    // ステートマシン
    readonly FieldBuilder _state;
    readonly FieldBuilder _builder;
    // 引数
    readonly FieldBuilder _command;
    readonly FieldBuilder _cancellationToken;

    const string _className = "_Selector";
    static int _no = 0;

    public static AsyncSelectGenerator Create<TEntity>(EntityMetaService metaService, ModuleBuilder moduleBuilder) where TEntity : class
    {
        var className = $"{_className}_{_no++}";
        var typeb = moduleBuilder.DefineType(className, TypeAttributes.Public, typeof(ValueType), new Type[] { typeof(IAsyncStateMachine) });

        var state = typeb.DefineField("_state", typeof(int), FieldAttributes.Private);

        var returnType = typeof(IReadOnlyList<>).MakeGenericType(typeof(TEntity));
        var returnImplementationType = typeof(List<>).MakeGenericType(typeof(TEntity));
        var returnValueTaskType = typeof(ValueTask<>).MakeGenericType(returnType);

        var builderType = typeof(AsyncValueTaskMethodBuilder<>).MakeGenericType(returnType);

        var builder = typeb.DefineField("_builder", builderType, FieldAttributes.Private);
        var command = typeb.DefineField("_command", typeof(DbCommand), FieldAttributes.Private);
        var cancellationToken = typeb.DefineField("_cancellationToken", typeof(CancellationToken), FieldAttributes.Private);

        return new(metaService, moduleBuilder, typeb, returnType, returnImplementationType, returnValueTaskType, builderType, state, builder, command, cancellationToken);
    }

    AsyncSelectGenerator(EntityMetaService metaService, ModuleBuilder moduleBuilder, TypeBuilder typeb, Type returnType, Type returnImplementationType, Type returnValueTaskType, Type builderType, FieldBuilder state, FieldBuilder builder, FieldBuilder command, FieldBuilder cancellationToken)
    {
        _metaService = metaService;
        _moduleBuilder = moduleBuilder;
        _typeb = typeb;
        _returnType = returnType;
        _returnImplementationType = returnImplementationType;
        _returnValueTaskType = returnValueTaskType;
        _builderType = builderType;
        _state = state;
        _builder = builder;
        _command = command;
        _cancellationToken = cancellationToken;
    }

    public Type Build<TEntity>(Query<TEntity> query) where TEntity : class
    {
        createExecuteMethod();
        createSetStateMachine();
        createMoveNextMethod(query);
        return _typeb.CreateType();
    }


    void createExecuteMethod()
    {
        var methodb = _typeb.DefineMethod("Execute", MethodAttributes.Public | MethodAttributes.Static, _returnValueTaskType, new Type[] { typeof(DbCommand), typeof(CancellationToken) });

        var il = methodb.GetILGenerator();
        var inst = il.DeclareLocal(_typeb);

        // inst = new XXX()
        il.EmitLdloca_S(inst);
        il.Emit(OpCodes.Initobj, _typeb);

        // inst._state = -1
        il.EmitLdloca_S(inst);
        il.Emit(OpCodes.Ldc_I4_M1);
        il.EmitStfld(_state);

        // inst._builder = AsyncValueTaskMethodBuilder<IReadOnlyList<TEntity>>.Create()
        il.EmitLdloca_S(inst);
        il.Emit(OpCodes.Call, _builderType.GetMethod("Create")!);
        il.EmitStfld(_builder);

        // inst._command = arg0
        il.EmitLdloca_S(inst);
        il.EmitLdarg(0);
        il.EmitStfld(_command);

        // inst._cancellationToken = arg1
        il.EmitLdloca_S(inst);
        il.EmitLdarg(1);
        il.EmitStfld(_cancellationToken);

        // inst._builder.Start(ref inst)
        il.EmitLdloca_S(inst);
        il.Emit(OpCodes.Ldflda, _builder);
        il.EmitLdloca_S(inst);
        il.EmitCall(_builderType.GetMethod("Start")!.MakeGenericMethod(_typeb));

        // return inst._builder.Task
        il.EmitLdloca_S(inst);
        il.Emit(OpCodes.Ldflda, _builder);
        il.EmitCall(_builderType.GetProperty("Task")!.GetGetMethod()!);
        il.Emit(OpCodes.Ret);
    }


    void createMoveNextMethod<TEntity>(Query<TEntity> query) where TEntity : class
    {
        var methodb = _typeb.DefineMethod(nameof(IAsyncStateMachine.MoveNext), MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final, null, Type.EmptyTypes);

        var il = methodb.GetILGenerator();

        var readerAwaiterType = typeof(TaskAwaiter<DbDataReader>);
        var boolAwaiterType = typeof(TaskAwaiter<bool>);

        var _ex = _typeb.DefineField("_ex", typeof(Exception), FieldAttributes.Private);
        var _ret = _typeb.DefineField("_ret", _returnType, FieldAttributes.Private);
        var _reader = _typeb.DefineField("_reader", typeof(DbDataReader), FieldAttributes.Private);
        var _readerAwaiter = _typeb.DefineField("_readerAwaiter", readerAwaiterType, FieldAttributes.Private);
        var _boolAwaiter = _typeb.DefineField("_boolAwaiter", boolAwaiterType, FieldAttributes.Private);


        var root = Root.Create(_metaService, query);
        var queryGenerator = new AsyncQueryGenerator(null, root.RootQuery);

        var queryCount = queryGenerator.GetQueryCount();

        var switchLabelCount = 3 * queryCount + 1;
        var switchLabels = new Label[switchLabelCount];
        for (var i = 0; i < switchLabelCount; i++)
            switchLabels[i] = il.DefineLabel();

        // var state = _state
        var state = il.DeclareLocal(typeof(int));
        il.EmitLdarg(0);
        il.EmitLdfld(_state);
        il.EmitStloc(state);

        // switch(state)
        var switchEnd = il.DefineLabel();
        var awaiter3Type = typeof(ValueTaskAwaiter);
        il.EmitLdloc(state);
        il.Emit(OpCodes.Switch, switchLabels);
        {
            // default:
            {
                // var reader = await command.ExecuteReaderAsync()
                // var awaiter1 = _awaiter1 = _command.ExecuteReaderAsync(_cancellationToken).GetAwaiter()
                var awaiter1 = il.DeclareLocal(readerAwaiterType);
                il.EmitLdarg(0);
                il.EmitLdarg(0);
                il.EmitLdfld(_command);
                il.EmitLdarg(0);
                il.EmitLdfld(_cancellationToken);

                il.EmitCall(DbCommand_ExecuteReaderAsync);
                il.EmitCall(typeof(Task<DbDataReader>).GetMethod("GetAwaiter")!);
                il.Emit(OpCodes.Dup);
                il.EmitStloc(awaiter1);
                il.EmitStfld(_readerAwaiter);

                // if(!awaiter1.IsCompleted)
                var ifEnd = il.DefineLabel();
                il.EmitLdloca_S(awaiter1);
                il.EmitCall(readerAwaiterType.GetProperty("IsCompleted")!.GetGetMethod()!);
                il.Emit(OpCodes.Brtrue_S, ifEnd);
                {
                    // _state = 0
                    il.EmitLdarg(0);
                    il.EmitLdc_I4(0);
                    il.EmitStfld(_state);

                    // _builder.AwaitUnsafeOnCompleted(ref awaiter1, ref this)
                    il.EmitLdarg(0);
                    il.Emit(OpCodes.Ldflda, _builder);
                    il.EmitLdloca_S(awaiter1);
                    il.EmitLdarg(0);
                    il.EmitCall(_builderType.GetMethod("AwaitUnsafeOnCompleted")!.MakeGenericMethod(readerAwaiterType, _typeb));

                    // return
                    il.Emit(OpCodes.Ret);
                }
                il.MarkLabel(ifEnd);

                // state = _state = 0
                il.EmitLdarg(0);
                il.EmitLdc_I4(0);
                il.Emit(OpCodes.Dup);
                il.EmitStloc(state);
                il.EmitStfld(_state);
            }
            // case 0:
            //   :
            // case ?:
            {
                foreach (var switchLabel in switchLabels.AsSpan(0, switchLabelCount - 1))
                    il.MarkLabel(switchLabel);

                var reader = il.DeclareLocal(typeof(DbDataReader));
                var ret = il.DeclareLocal(_returnImplementationType!);
                var awaiter2 = il.DeclareLocal(boolAwaiterType);

                // try
                il.BeginExceptionBlock();
                {
                    // switch(state)
                    var switchLabel2Count = 3 * queryCount;
                    var switchLabel2s = new Label[switchLabel2Count];
                    for (var i = 0; i < switchLabel2Count; i++)
                        switchLabel2s[i] = il.DefineLabel();
                    var switch2End = il.DefineLabel();

                    il.EmitLdloc(state);
                    il.Emit(OpCodes.Switch, switchLabel2s);
                    // default:
                    {
                        il.Emit(OpCodes.Newobj, typeof(InvalidProgramException).GetConstructor(Type.EmptyTypes)!);
                        il.Emit(OpCodes.Throw);
                    }

                    var queryGenerators = new List<AsyncQueryGenerator>();
                    queryGenerator.GetAllQueries(queryGenerators);
                    var index = 0;
                    var dictionaryIndex = 0;
                    var isFirst = true;
                    foreach (var queryGenerator2 in queryGenerators)
                    {
                        if (isFirst)
                        {
                            // case 0:
                            il.MarkLabel(switchLabel2s[index]);

                            // _reader = _awaiter1.GetResult()
                            il.EmitLdarg(0);
                            il.EmitLdarg(0);
                            il.Emit(OpCodes.Ldflda, _readerAwaiter);
                            il.EmitCall(readerAwaiterType.GetMethod("GetResult")!);
                            il.EmitStfld(_reader);

                            // _list = new List<TEntity>()
                            il.EmitLdarg(0);
                            il.Emit(OpCodes.Newobj, Cast<TEntity>.List_New);
                            il.EmitStfld(_ret!);

                            queryGenerator2.EmitInit(_typeb, il, ref dictionaryIndex);

                            // state = _state = 1
                            il.EmitLdarg(0);
                            il.EmitLdc_I4(index + 1);
                            il.Emit(OpCodes.Dup);
                            il.EmitStloc(state);
                            il.EmitStfld(_state);

                            index++;
                            isFirst = false;
                        }
                        else
                        {
                            // case 3:
                            il.MarkLabel(switchLabel2s[index]);

                            // _awaiter2.GetResult()
                            il.EmitLdarg(0);
                            il.Emit(OpCodes.Ldflda, _boolAwaiter);
                            il.EmitCall(boolAwaiterType.GetMethod("GetResult")!);
                            il.Emit(OpCodes.Pop);

                            queryGenerator2.EmitInit(_typeb, il, ref dictionaryIndex);

                            // state = _state = 1
                            il.EmitLdarg(0);
                            il.EmitLdc_I4(index + 1);
                            il.Emit(OpCodes.Dup);
                            il.EmitStloc(state);
                            il.EmitStfld(_state);

                            index++;
                        }
                        // case 1:
                        {
                            il.MarkLabel(switchLabel2s[index]);

                            // while(await reader.ReadAsync())
                            // var awaiter2 = _awaiter2 = _reader.ReadAsync(_cancellationToken).GetAwaiter();
                            il.EmitLdarg(0);
                            il.EmitLdarg(0);
                            il.EmitLdfld(_reader);
                            il.EmitLdarg(0);
                            il.EmitLdfld(_cancellationToken);
                            il.Emit(OpCodes.Callvirt, DbDataReader_ReadAsync);
                            il.Emit(OpCodes.Callvirt, typeof(Task<bool>).GetMethod("GetAwaiter")!);
                            il.Emit(OpCodes.Dup);
                            il.EmitStloc(awaiter2);
                            il.EmitStfld(_boolAwaiter);

                            // if(!awaiter2.IsCompleted)
                            var ifEnd = il.DefineLabel();
                            il.EmitLdloca_S(awaiter2);
                            il.EmitCall(boolAwaiterType.GetProperty("IsCompleted")!.GetGetMethod()!);
                            il.Emit(OpCodes.Brtrue_S, ifEnd);
                            {
                                // _state = 2
                                il.EmitLdarg(0);
                                il.EmitLdc_I4(index + 1);
                                il.EmitStfld(_state);

                                // _builder.AwaitUnsafeOnCompleted(ref awaiter2, ref this)
                                il.EmitLdarg(0);
                                il.Emit(OpCodes.Ldflda, _builder);
                                il.EmitLdloca_S(awaiter2);
                                il.EmitLdarg(0);
                                il.Emit(OpCodes.Call, _builderType.GetMethod("AwaitUnsafeOnCompleted")!.MakeGenericMethod(boolAwaiterType, _typeb));

                                // return
                                il.Emit(OpCodes.Leave, switchEnd);
                            }
                            il.MarkLabel(ifEnd);

                            // state = _state = 2
                            il.EmitLdarg(0);
                            il.EmitLdc_I4(index + 1);
                            il.Emit(OpCodes.Dup);
                            il.EmitStloc(state);
                            il.EmitStfld(_state);

                            index++;
                        }
                        // case 2:
                        {
                            il.MarkLabel(switchLabel2s[index]);

                            // if (!_awaiter2.GetResult())
                            var ifEnd = il.DefineLabel();
                            il.EmitLdarg(0);
                            il.Emit(OpCodes.Ldflda, _boolAwaiter);
                            il.EmitCall(boolAwaiterType.GetMethod("GetResult")!);
                            il.Emit(OpCodes.Brtrue, ifEnd);
                            {
                                if (queryGenerator2 == queryGenerators.Last())
                                {
                                    // break
                                    il.Emit(OpCodes.Br, switch2End);
                                }
                                else
                                {
                                    // var awaiter2 = _awaiter2 = _reader.NextResultAsync(_cancellationToken).GetAwaiter();
                                    il.EmitLdarg(0);
                                    il.EmitLdarg(0);
                                    il.EmitLdfld(_reader);
                                    il.EmitLdarg(0);
                                    il.EmitLdfld(_cancellationToken);
                                    il.Emit(OpCodes.Callvirt, DbDataReader_NextResultAsync);
                                    il.Emit(OpCodes.Callvirt, typeof(Task<bool>).GetMethod("GetAwaiter")!);
                                    il.Emit(OpCodes.Dup);
                                    il.EmitStloc(awaiter2);
                                    il.EmitStfld(_boolAwaiter);

                                    // if(!awaiter2.IsCompleted)
                                    var ifEnd2 = il.DefineLabel();
                                    il.EmitLdloca_S(awaiter2);
                                    il.EmitCall(boolAwaiterType.GetProperty("IsCompleted")!.GetGetMethod()!);
                                    il.Emit(OpCodes.Brtrue_S, ifEnd2);
                                    {
                                        // _state = 3
                                        il.EmitLdarg(0);
                                        il.EmitLdc_I4(index + 1);
                                        il.EmitStfld(_state);

                                        // _builder.AwaitUnsafeOnCompleted(ref awaiter2, ref this)
                                        il.EmitLdarg(0);
                                        il.Emit(OpCodes.Ldflda, _builder);
                                        il.EmitLdloca_S(awaiter2);
                                        il.EmitLdarg(0);
                                        il.Emit(OpCodes.Call, _builderType.GetMethod("AwaitUnsafeOnCompleted")!.MakeGenericMethod(boolAwaiterType, _typeb));

                                        // return
                                        il.Emit(OpCodes.Leave, switchEnd);
                                    }
                                    il.MarkLabel(ifEnd2);

                                    // state = _state = 3;
                                    il.EmitLdarg(0);
                                    il.EmitLdc_I4(index + 1);
                                    il.Emit(OpCodes.Dup);
                                    il.EmitStloc(state);
                                    il.EmitStfld(_state);

                                    // goto case 3:
                                    il.Emit(OpCodes.Br, switchLabel2s[index + 1]);
                                }
                            }
                            il.MarkLabel(ifEnd);

                            // while inner
                            // var reader = _reader
                            il.EmitLdarg(0);
                            il.EmitLdfld(_reader);
                            il.EmitStloc(reader);

                            if (queryGenerator2.Parent == null)
                            {
                                il.EmitLdarg(0);
                                il.EmitLdfld(_ret!);
                                il.EmitStloc(ret);
                            }

                            queryGenerator2.EmitBody(il, reader, ret);

                            // state = _state = 1;
                            il.EmitLdarg(0);
                            il.EmitLdc_I4(index - 1);
                            il.Emit(OpCodes.Dup);
                            il.EmitStloc(state);
                            il.EmitStfld(_state);

                            // goto case 1:
                            il.Emit(OpCodes.Br, switchLabel2s[index - 1]);

                            index++;
                        }
                    }
                    il.MarkLabel(switch2End);
                }
                // catch
                il.BeginCatchBlock(typeof(Exception));
                {
                    // _ex = ex
                    var ex = il.DeclareLocal(typeof(Exception));
                    il.EmitStloc(ex);
                    il.EmitLdarg(0);
                    il.EmitLdloc(ex);
                    il.EmitStfld(_ex);
                }
                il.EndExceptionBlock();

                // finally inner
                {
                    // var awaiter3  = _reader.DisposeAsync().GetAwaiter()
                    var valueTask = il.DeclareLocal(typeof(ValueTask));
                    var awaiter3 = il.DeclareLocal(awaiter3Type);
                    il.EmitLdarg(0);
                    il.EmitLdfld(_reader);
                    il.EmitCall(DbDataReader_DisposeAsync);
                    il.EmitStloc(valueTask);
                    il.EmitLdloca_S(valueTask);
                    il.EmitCall(typeof(ValueTask).GetMethod("GetAwaiter")!);
                    il.EmitStloc(awaiter3);

                    // if(!awaiter3.IsCompleted)
                    var ifEnd = il.DefineLabel();
                    il.EmitLdloca_S(awaiter3);
                    il.EmitCall(awaiter3Type.GetProperty("IsCompleted")!.GetGetMethod()!);
                    il.Emit(OpCodes.Brtrue_S, ifEnd);
                    {
                        // _state = 3
                        il.EmitLdarg(0);
                        il.EmitLdc_I4(switchLabelCount - 1);
                        il.EmitStfld(_state);

                        // _builder.AwaitUnsafeOnCompleted(ref awaiter2, ref this)
                        il.EmitLdarg(0);
                        il.Emit(OpCodes.Ldflda, _builder);
                        il.EmitLdloca_S(awaiter3);
                        il.EmitLdarg(0);
                        il.EmitCall(_builderType.GetMethod("AwaitUnsafeOnCompleted")!.MakeGenericMethod(awaiter3Type, _typeb));

                        // return
                        il.Emit(OpCodes.Ret);
                    }
                    il.MarkLabel(ifEnd);
                }
            }
            // case last:
            {
                il.MarkLabel(switchLabels[switchLabelCount - 1]);

                // if (_ex != null)
                var ifEnd = il.DefineLabel();
                il.EmitLdarg(0);
                il.EmitLdfld(_ex);
                il.Emit(OpCodes.Brfalse_S, ifEnd);
                {
                    // throw _ex
                    il.EmitLdarg(0);
                    il.EmitLdfld(_ex);
                    il.Emit(OpCodes.Throw);
                }
                il.MarkLabel(ifEnd);

                // _builder.SetResult(_ret)
                il.EmitLdarg(0);
                il.Emit(OpCodes.Ldflda, _builder);
                il.EmitLdarg(0);
                il.EmitLdfld(_ret!);
                il.EmitCall(_builderType.GetMethod("SetResult")!);
            }
        }
        il.MarkLabel(switchEnd);

        il.Emit(OpCodes.Ret);
    }

    void createSetStateMachine()
    {
        var methodb = _typeb.DefineMethod(nameof(IAsyncStateMachine.SetStateMachine), MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final, null, new[] { typeof(IAsyncStateMachine) });

        var il = methodb.GetILGenerator();

        // _builder.SetStateMachine(stateMachine)
        il.EmitLdarg(0);
        il.Emit(OpCodes.Ldflda, _builder);
        il.EmitLdarg(1);
        il.EmitCall(_builderType.GetMethod("SetStateMachine")!);

        // return
        il.Emit(OpCodes.Ret);
    }
}
