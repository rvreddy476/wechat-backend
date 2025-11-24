using MongoDB.Driver;
using Shared.Domain.Common;
using System.Linq.Expressions;

namespace Shared.Infrastructure.MongoDB;

public interface IMongoRepository<TEntity> where TEntity : class, IEntity
{
    Task<TEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);
    Task<TEntity?> FindOneAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);
    Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task<long> CountAsync(Expression<Func<TEntity, bool>>? filter = null, CancellationToken cancellationToken = default);
    IMongoCollection<TEntity> Collection { get; }
}

public class MongoRepository<TEntity> : IMongoRepository<TEntity> where TEntity : class, IEntity
{
    private readonly IMongoCollection<TEntity> _collection;
    private readonly string _idFieldName;

    public MongoRepository(IMongoDatabase database, string collectionName, string idFieldName = "_id")
    {
        _collection = database.GetCollection<TEntity>(collectionName);
        _idFieldName = idFieldName;
    }

    public IMongoCollection<TEntity> Collection => _collection;

    public virtual async Task<TEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<TEntity>.Filter.Eq(_idFieldName, id);
        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _collection.Find(_ => true).ToListAsync(cancellationToken);
    }

    public virtual async Task<List<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken = default)
    {
        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }

    public virtual async Task<TEntity?> FindOneAsync(
        Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken = default)
    {
        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        await _collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
        return entity;
    }

    public virtual async Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;

        var idProperty = typeof(TEntity).GetProperty("Id")
            ?? typeof(TEntity).GetProperties().FirstOrDefault(p => p.Name.EndsWith("Id"));

        if (idProperty == null)
            return false;

        var idValue = idProperty.GetValue(entity)?.ToString();
        if (string.IsNullOrEmpty(idValue))
            return false;

        var filter = Builders<TEntity>.Filter.Eq(_idFieldName, idValue);
        var result = await _collection.ReplaceOneAsync(filter, entity, cancellationToken: cancellationToken);

        return result.ModifiedCount > 0;
    }

    public virtual async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<TEntity>.Filter.Eq(_idFieldName, id);
        var result = await _collection.DeleteOneAsync(filter, cancellationToken);
        return result.DeletedCount > 0;
    }

    public virtual async Task<long> CountAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        CancellationToken cancellationToken = default)
    {
        return filter == null
            ? await _collection.CountDocumentsAsync(FilterDefinition<TEntity>.Empty, cancellationToken: cancellationToken)
            : await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
    }
}
