using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaveSenseAI.Domain.Entities;

namespace SaveSenseAI.Infrastructure.Persistence.Configurations;

public class CouponValidationAttemptConfiguration : IEntityTypeConfiguration<CouponValidationAttempt>
{
    public void Configure(EntityTypeBuilder<CouponValidationAttempt> builder)
    {
        builder.ToTable("CouponValidationAttempts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.AttemptedCode).IsRequired().HasMaxLength(64);
        builder.Property(a => a.CartSubtotal).HasPrecision(18, 2);
        builder.Property(a => a.DiscountAmount).HasPrecision(18, 2);
        builder.Property(a => a.FailureReason).HasConversion<string>().HasMaxLength(32);

        builder.HasIndex(a => a.StoreId);
        builder.HasIndex(a => a.AttemptedAtUtc);

        // StoreId is required, so it cascades (a deleted store takes its
        // attempt history with it). CouponId and UserId are optional
        // references on this audit table — if the coupon or user is later
        // deleted, the attempt row survives with that link set to null
        // rather than losing the record of what was tried.
        builder.HasOne<Store>()
            .WithMany()
            .HasForeignKey(a => a.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Coupon>()
            .WithMany()
            .HasForeignKey(a => a.CouponId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
