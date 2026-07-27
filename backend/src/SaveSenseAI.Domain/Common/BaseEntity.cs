namespace SaveSenseAI.Domain.Common;

/// <summary>
/// Base type for all domain entities. Identity-based equality: two entities
/// are equal if they are the same type and have the same non-empty Id,
/// regardless of what their other properties currently hold.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();

    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity other || other.GetType() != GetType())
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (Id == Guid.Empty || other.Id == Guid.Empty)
        {
            return false;
        }

        return Id == other.Id;
    }

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(BaseEntity? left, BaseEntity? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(BaseEntity? left, BaseEntity? right) => !(left == right);
}
