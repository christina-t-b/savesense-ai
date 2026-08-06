using MediatR;
using Microsoft.EntityFrameworkCore;
using SaveSenseAI.Application.Common.Interfaces;
using SaveSenseAI.Domain.Entities;

namespace SaveSenseAI.Application.Coupons.Commands.ValidateCoupon;

public sealed class ValidateCouponCommandHandler(
    IApplicationDbContext dbContext,
    IDateTime dateTime,
    ICurrentUserService currentUserService)
    : IRequestHandler<ValidateCouponCommand, CouponValidationResult>
{
    public async Task<CouponValidationResult> Handle(ValidateCouponCommand request, CancellationToken cancellationToken)
    {
        var now = dateTime.UtcNow;
        var normalizedCode = request.Code.Trim().ToUpperInvariant();

        var coupon = await dbContext.Coupons
            .SingleOrDefaultAsync(c => c.StoreId == request.StoreId && c.Code == normalizedCode, cancellationToken);

        var outcome = coupon is null
            ? CouponValidationOutcome.Failure(CouponFailureReason.NotFound)
            : coupon.Validate(request.CartSubtotal, now);

        if (coupon is not null && outcome.IsSuccess)
        {
            coupon.RecordRedemption();
        }

        var attempt = CouponValidationAttempt.Record(
            request.StoreId,
            coupon?.Id,
            normalizedCode,
            request.CartSubtotal,
            currentUserService.UserId,
            outcome,
            now);

        dbContext.CouponValidationAttempts.Add(attempt);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CouponValidationResult(outcome.IsSuccess, outcome.DiscountAmount, outcome.FailureReason);
    }
}
