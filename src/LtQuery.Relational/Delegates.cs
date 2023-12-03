using System.Data.Common;

namespace LtQuery.Relational;

public delegate IReadOnlyList<TEntity> ExecuteSelect<TEntity>(DbCommand command) where TEntity : class;
public delegate IReadOnlyList<TEntity> ExecuteSelect<TEntity, TParameter>(DbCommand command, TParameter parameters) where TEntity : class;
public delegate int ExecuteCount(DbCommand command);
public delegate int ExecuteCount<TParameter>(DbCommand command, TParameter parameters);
public delegate TEntity ExecuteSingle<TEntity>(DbCommand command) where TEntity : class;
public delegate TEntity ExecuteSingle<TEntity, TParameter>(DbCommand command, TParameter parameters) where TEntity : class;
public delegate void ExecuteUpdate<TEntity>(DbCommand command, Span<TEntity> entities) where TEntity : class;
