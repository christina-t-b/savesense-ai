namespace SaveSenseAI.Domain.Entities;

/// <summary>Result of checking a coupon against a cart — not persisted
/// itself, just the pure computation. CouponValidationAttempt is what
/// records it.</summary>
public sealed record CouponValidationOutcome(bool IsSuccess, decimal? DiscountAmount, CouponFailureReason? FailureReason)
{
    public static CouponValidationOutcome Success(decimal discountAmount) => new(true, discountAmount, null);

    public static CouponValidationOutcome Failure(CouponFailureReason reason) => new(false, null, reason);
}
