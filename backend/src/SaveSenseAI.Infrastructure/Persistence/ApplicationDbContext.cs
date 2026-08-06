using Microsoft.EntityFrameworkCore;
using SaveSenseAI.Application.Common.Interfaces;
using SaveSenseAI.Domain.Entities;

namespace SaveSenseAI.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<CouponValidationAttempt> CouponValidationAttempts => Set<CouponValidationAttempt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
