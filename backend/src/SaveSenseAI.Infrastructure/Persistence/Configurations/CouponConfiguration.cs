using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaveSenseAI.Domain.Entities;

namespace SaveSenseAI.Infrastructure.Persistence.Configurations;

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.ToTable("Coupons");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code).IsRequired().HasMaxLength(64);
        builder.Property(c => c.Description).IsRequired().HasMaxLength(500);

        builder.Property(c => c.DiscountType).HasConversion<string>().HasMaxLength(32);
        builder.Property(c => c.DiscountValue).HasPrecision(18, 2);
        builder.Property(c => c.MinimumSpendAmount).HasPrecision(18, 2);

        // A code only has to be unique per store, not globally — two
        // different retailers can both happen to run "SAVE20".
        builder.HasIndex(c => new { c.StoreId, c.Code }).IsUnique();

        builder.HasOne<Store>()
            .WithMany()
            .HasForeignKey(c => c.StoreId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
