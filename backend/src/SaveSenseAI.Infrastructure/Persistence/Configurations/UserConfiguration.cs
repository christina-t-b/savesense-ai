using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaveSenseAI.Domain.Entities;

namespace SaveSenseAI.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.DisplayName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.GoogleSubjectId)
            .IsRequired()
            .HasMaxLength(64);

        builder.HasIndex(u => u.GoogleSubjectId).IsUnique();

        builder.Property(u => u.CreatedAtUtc).IsRequired();
    }
}
