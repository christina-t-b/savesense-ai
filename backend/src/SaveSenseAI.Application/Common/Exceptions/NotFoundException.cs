namespace SaveSenseAI.Application.Common.Exceptions;

/// <summary>Thrown when a request references an entity that doesn't exist.
/// Mapped to HTTP 404 by the API layer.</summary>
public sealed class NotFoundException(string entityName, object key)
    : Exception($"{entityName} with id '{key}' was not found.");
