using SaveSenseAI.Domain.Common;

namespace SaveSenseAI.Domain.Entities;

/// <summary>
/// A permanent record of every "does this code work" check, successful or
/// not — this is what makes explaining a failure possible after the fact,
/// instead of the reason only ever existing as a transient API response.
/// </summary>
public class CouponValidationAttempt : BaseEntity
{
    public Guid StoreId { get; private set; }
    public Guid? CouponId { get; private set; }
    public string AttemptedCode { get; private set; } = null!;
    public decimal CartSubtotal { get; private set; }
    public Guid? UserId { get; private set; }
    public bool IsSuccess { get; private set; }
    public CouponFailureReason? FailureReason { get; private set; }
    public decimal? DiscountAmount { get; private set; }
    public DateTimeOffset AttemptedAtUtc { get; private set; }

    private CouponValidationAttempt() { }

    public static CouponValidationAttempt Record(
        Guid storeId,
        Guid? couponId,
        string attemptedCode,
        decimal cartSubtotal,
        Guid? userId,
        CouponValidationOutcome outcome,
        DateTimeOffset nowUtc)
    {
        return new CouponValidationAttempt
        {
            StoreId = storeId,
            CouponId = couponId,
            AttemptedCode = attemptedCode,
            CartSubtotal = cartSubtotal,
            UserId = userId,
            IsSuccess = outcome.IsSuccess,
            FailureReason = outcome.FailureReason,
            DiscountAmount = outcome.DiscountAmount,
            AttemptedAtUtc = nowUtc,
        };
    }
}
