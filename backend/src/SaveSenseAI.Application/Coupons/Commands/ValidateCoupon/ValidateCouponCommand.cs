using MediatR;
using SaveSenseAI.Domain.Entities;

namespace SaveSenseAI.Application.Coupons.Commands.ValidateCoupon;

public sealed record ValidateCouponCommand(Guid StoreId, string Code, decimal CartSubtotal) : IRequest<CouponValidationResult>;

public sealed record CouponValidationResult(bool IsSuccess, decimal? DiscountAmount, CouponFailureReason? FailureReason);
