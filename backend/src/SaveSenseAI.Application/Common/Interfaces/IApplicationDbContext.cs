using Microsoft.EntityFrameworkCore;
using SaveSenseAI.Domain.Entities;

namespace SaveSenseAI.Application.Common.Interfaces;

/// <summary>
/// What the Application layer is allowed to know about persistence — just
/// the DbSets it needs and SaveChanges. Infrastructure owns everything else
/// (connection strings, migrations, EF configuration).
/// </summary>
public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Store> Stores { get; }
    DbSet<Coupon> Coupons { get; }
    DbSet<CouponValidationAttempt> CouponValidationAttempts { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
