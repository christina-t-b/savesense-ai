namespace SaveSenseAI.Domain.Entities;

public enum CouponFailureReason
{
    /// <summary>No coupon with this code exists for this store. Set by the
    /// Application layer — Coupon.Validate is only ever called on a coupon
    /// that was already found, so it never returns this value itself.</summary>
    NotFound,
    Inactive,
    Expired,
    RedemptionLimitReached,
    MinimumSpendNotMet,
}
