namespace LtQuery;

public interface IDbUpdater
{
    /// <summary>
    /// add entity
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="entity"></param>
    void Add<TEntity>(TEntity entity) where TEntity : class;

    /// <summary>
    /// add entities
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="entities"></param>
    void AddRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;

    /// <summary>
    /// update entity
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="entity"></param>
    void Update<TEntity>(TEntity entity) where TEntity : class;

    /// <summary>
    /// update entities
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="entities"></param>
    void UpdateRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;

    /// <summary>
    /// remove entity
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="entity"></param>
    void Remove<TEntity>(TEntity entity) where TEntity : class;

    /// <summary>
    /// remove entities
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="entities"></param>
    void RemoveRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;
}
