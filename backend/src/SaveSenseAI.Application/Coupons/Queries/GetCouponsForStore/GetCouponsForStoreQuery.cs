using MediatR;
using SaveSenseAI.Domain.Entities;

namespace SaveSenseAI.Application.Coupons.Queries.GetCouponsForStore;

public sealed record GetCouponsForStoreQuery(Guid StoreId) : IRequest<IReadOnlyList<CouponSummary>>;

public sealed record CouponSummary(
    Guid Id,
    string Code,
    string Description,
    DiscountType DiscountType,
    decimal DiscountValue,
    decimal? MinimumSpendAmount,
    DateTimeOffset? ExpiresAtUtc);
