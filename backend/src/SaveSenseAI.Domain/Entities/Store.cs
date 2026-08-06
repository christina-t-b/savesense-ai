using SaveSenseAI.Domain.Common;

namespace SaveSenseAI.Domain.Entities;

public class Store : BaseEntity
{
    public string Name { get; private set; } = null!;

    /// <summary>URL-safe identifier (e.g. "nike"), used for lookups
    /// independent of display-name changes.</summary>
    public string Slug { get; private set; } = null!;

    public bool IsActive { get; private set; }

    private Store() { }

    public static Store Create(string name, string slug)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new ArgumentException("Slug is required.", nameof(slug));
        }

        return new Store
        {
            Name = name,
            Slug = slug,
            IsActive = true,
        };
    }
}
