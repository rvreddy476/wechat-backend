namespace Shared.Domain.Common;

/// <summary>
/// Base interface for all entities
/// </summary>
public interface IEntity
{
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Base interface for entities with string ID (MongoDB uses string IDs)
/// </summary>
public interface IEntity<TId> : IEntity where TId : notnull
{
    TId Id { get; }
}

/// <summary>
/// Base interface for aggregate roots in DDD
/// </summary>
public interface IAggregateRoot : IEntity
{
}

/// <summary>
/// Interface for soft delete support
/// </summary>
public interface ISoftDelete
{
    bool IsDeleted { get; }
    DateTime? DeletedAt { get; }
}

/// <summary>
/// Base entity class for MongoDB documents
/// </summary>
public abstract class Entity : IEntity
{
    public virtual DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public virtual DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Base entity class with ID for MongoDB documents
/// </summary>
public abstract class Entity<TId> : Entity, IEntity<TId> where TId : notnull
{
    public virtual TId Id { get; set; } = default!;
}

/// <summary>
/// Base aggregate root for domain-driven design
/// </summary>
public abstract class AggregateRoot : Entity, IAggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
