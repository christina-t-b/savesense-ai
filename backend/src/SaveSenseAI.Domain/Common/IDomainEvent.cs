namespace SaveSenseAI.Domain.Common;

/// <summary>
/// Marker for something that happened in the domain that other parts of the
/// system (e.g. Infrastructure event handlers) may need to react to.
/// </summary>
public interface IDomainEvent
{
    DateTimeOffset OccurredOn { get; }
}
