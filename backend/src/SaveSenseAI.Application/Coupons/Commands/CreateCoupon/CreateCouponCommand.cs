using MediatR;
using SaveSenseAI.Domain.Entities;

namespace SaveSenseAI.Application.Coupons.Commands.CreateCoupon;

public sealed record CreateCouponCommand(
    Guid StoreId,
    string Code,
    string Description,
    DiscountType DiscountType,
    decimal DiscountValue,
    decimal? MinimumSpendAmount,
    DateTimeOffset? ExpiresAtUtc,
    int? MaxRedemptions) : IRequest<Guid>;
