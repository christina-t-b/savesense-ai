using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaveSenseAI.Domain.Entities;

namespace SaveSenseAI.Infrastructure.Persistence.Configurations;

public class StoreConfiguration : IEntityTypeConfiguration<Store>
{
    public void Configure(EntityTypeBuilder<Store> builder)
    {
        builder.ToTable("Stores");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name).IsRequired().HasMaxLength(200);

        builder.Property(s => s.Slug).IsRequired().HasMaxLength(200);
        builder.HasIndex(s => s.Slug).IsUnique();
    }
}
