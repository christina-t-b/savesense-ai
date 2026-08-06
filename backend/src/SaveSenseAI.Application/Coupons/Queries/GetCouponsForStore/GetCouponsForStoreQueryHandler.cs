using MediatR;
using Microsoft.EntityFrameworkCore;
using SaveSenseAI.Application.Common.Interfaces;

namespace SaveSenseAI.Application.Coupons.Queries.GetCouponsForStore;

public sealed class GetCouponsForStoreQueryHandler(IApplicationDbContext dbContext, IDateTime dateTime)
    : IRequestHandler<GetCouponsForStoreQuery, IReadOnlyList<CouponSummary>>
{
    public async Task<IReadOnlyList<CouponSummary>> Handle(GetCouponsForStoreQuery request, CancellationToken cancellationToken)
    {
        var now = dateTime.UtcNow;

        return await dbContext.Coupons
            .Where(c => c.StoreId == request.StoreId)
            .Where(c => c.IsActive)
            .Where(c => c.ExpiresAtUtc == null || c.ExpiresAtUtc > now)
            .Where(c => c.MaxRedemptions == null || c.RedemptionCount < c.MaxRedemptions)
            .OrderByDescending(c => c.DiscountValue)
            .Select(c => new CouponSummary(
                c.Id, c.Code, c.Description, c.DiscountType, c.DiscountValue, c.MinimumSpendAmount, c.ExpiresAtUtc))
            .ToListAsync(cancellationToken);
    }
}
