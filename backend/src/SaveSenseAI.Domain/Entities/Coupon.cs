using SaveSenseAI.Domain.Common;

namespace SaveSenseAI.Domain.Entities;

public class Coupon : BaseEntity
{
    public Guid StoreId { get; private set; }
    public string Code { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public DiscountType DiscountType { get; private set; }
    public decimal DiscountValue { get; private set; }
    public decimal? MinimumSpendAmount { get; private set; }
    public DateTimeOffset? ExpiresAtUtc { get; private set; }
    public int? MaxRedemptions { get; private set; }
    public int RedemptionCount { get; private set; }
    public bool IsActive { get; private set; }

    private Coupon() { }

    public static Coupon Create(
        Guid storeId,
        string code,
        string description,
        DiscountType discountType,
        decimal discountValue,
        decimal? minimumSpendAmount,
        DateTimeOffset? expiresAtUtc,
        int? maxRedemptions)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Code is required.", nameof(code));
        }

        if (discountValue <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(discountValue), "Discount value must be positive.");
        }

        if (discountType == DiscountType.Percentage && discountValue > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(discountValue), "A percentage discount cannot exceed 100.");
        }

        return new Coupon
        {
            StoreId = storeId,
            Code = code.Trim().ToUpperInvariant(),
            Description = description,
            DiscountType = discountType,
            DiscountValue = discountValue,
            MinimumSpendAmount = minimumSpendAmount,
            ExpiresAtUtc = expiresAtUtc,
            MaxRedemptions = maxRedemptions,
            RedemptionCount = 0,
            IsActive = true,
        };
    }

    /// <summary>
    /// We have no real checkout to confirm a redemption against — validating
    /// a code IS the terminal action in this system, so a successful
    /// validation counts as a redemption. A platform with real order data
    /// would separate "check eligibility" from "confirm redemption"
    /// (e.g. via an affiliate postback); this is a deliberate simplification
    /// for the data we actually have.
    /// </summary>
    public CouponValidationOutcome Validate(decimal cartSubtotal, DateTimeOffset nowUtc)
    {
        if (!IsActive)
        {
            return CouponValidationOutcome.Failure(CouponFailureReason.Inactive);
        }

        if (ExpiresAtUtc.HasValue && nowUtc >= ExpiresAtUtc.Value)
        {
            return CouponValidationOutcome.Failure(CouponFailureReason.Expired);
        }

        if (MaxRedemptions.HasValue && RedemptionCount >= MaxRedemptions.Value)
        {
            return CouponValidationOutcome.Failure(CouponFailureReason.RedemptionLimitReached);
        }

        if (MinimumSpendAmount.HasValue && cartSubtotal < MinimumSpendAmount.Value)
        {
            return CouponValidationOutcome.Failure(CouponFailureReason.MinimumSpendNotMet);
        }

        var discountAmount = DiscountType switch
        {
            DiscountType.Percentage => Math.Round(cartSubtotal * (DiscountValue / 100m), 2, MidpointRounding.ToEven),
            DiscountType.FixedAmount => Math.Min(DiscountValue, cartSubtotal),
            DiscountType.FreeShipping => 0m,
            _ => throw new InvalidOperationException($"Unhandled discount type '{DiscountType}'."),
        };

        return CouponValidationOutcome.Success(discountAmount);
    }

    public void RecordRedemption() => RedemptionCount++;
}
