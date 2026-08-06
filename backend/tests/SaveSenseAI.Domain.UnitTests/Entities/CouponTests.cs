using SaveSenseAI.Domain.Entities;

namespace SaveSenseAI.Domain.UnitTests.Entities;

public class CouponTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly Guid StoreId = Guid.NewGuid();

    private static Coupon CreatePercentageCoupon(
        decimal discountValue = 20,
        decimal? minimumSpendAmount = null,
        DateTimeOffset? expiresAtUtc = null,
        int? maxRedemptions = null) =>
        Coupon.Create(StoreId, "SAVE20", "20% off", DiscountType.Percentage, discountValue, minimumSpendAmount, expiresAtUtc, maxRedemptions);

    [Fact]
    public void Create_NormalizesCodeToUppercaseTrimmed()
    {
        var coupon = Coupon.Create(StoreId, "  save20  ", "desc", DiscountType.Percentage, 20, null, null, null);

        Assert.Equal("SAVE20", coupon.Code);
    }

    [Fact]
    public void Create_PercentageOver100_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Coupon.Create(StoreId, "TOOMUCH", "desc", DiscountType.Percentage, 150, null, null, null));
    }

    [Fact]
    public void Create_ZeroOrNegativeDiscount_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Coupon.Create(StoreId, "FREE", "desc", DiscountType.FixedAmount, 0, null, null, null));
    }

    [Fact]
    public void Validate_PercentageDiscount_ComputesCorrectAmount()
    {
        var coupon = CreatePercentageCoupon(discountValue: 20);

        var result = coupon.Validate(cartSubtotal: 100m, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(20m, result.DiscountAmount);
    }

    [Fact]
    public void Validate_FixedAmountDiscount_IsCappedAtCartSubtotal()
    {
        var coupon = Coupon.Create(StoreId, "TAKE50", "desc", DiscountType.FixedAmount, 50m, null, null, null);

        var result = coupon.Validate(cartSubtotal: 30m, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(30m, result.DiscountAmount); // never discounts more than the cart is worth
    }

    [Fact]
    public void Validate_FreeShipping_SucceedsWithZeroCartDiscount()
    {
        var coupon = Coupon.Create(StoreId, "FREESHIP", "desc", DiscountType.FreeShipping, 1m, null, null, null);

        var result = coupon.Validate(cartSubtotal: 100m, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(0m, result.DiscountAmount);
    }

    [Fact]
    public void Validate_Expired_FailsWithExpiredReason()
    {
        var coupon = CreatePercentageCoupon(expiresAtUtc: Now.AddDays(-1));

        var result = coupon.Validate(100m, Now);

        Assert.False(result.IsSuccess);
        Assert.Equal(CouponFailureReason.Expired, result.FailureReason);
    }

    [Fact]
    public void Validate_ExactlyAtExpiry_Fails()
    {
        // >= is intentional: a coupon should not be usable in the exact
        // instant it expires.
        var coupon = CreatePercentageCoupon(expiresAtUtc: Now);

        var result = coupon.Validate(100m, Now);

        Assert.False(result.IsSuccess);
        Assert.Equal(CouponFailureReason.Expired, result.FailureReason);
    }

    [Fact]
    public void Validate_BelowMinimumSpend_FailsWithMinimumSpendReason()
    {
        var coupon = CreatePercentageCoupon(minimumSpendAmount: 50m);

        var result = coupon.Validate(cartSubtotal: 49.99m, Now);

        Assert.False(result.IsSuccess);
        Assert.Equal(CouponFailureReason.MinimumSpendNotMet, result.FailureReason);
    }

    [Fact]
    public void Validate_AtExactMinimumSpend_Succeeds()
    {
        var coupon = CreatePercentageCoupon(minimumSpendAmount: 50m);

        var result = coupon.Validate(cartSubtotal: 50m, Now);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Validate_RedemptionLimitReached_FailsWithLimitReason()
    {
        var coupon = CreatePercentageCoupon(maxRedemptions: 1);
        coupon.RecordRedemption();

        var result = coupon.Validate(100m, Now);

        Assert.False(result.IsSuccess);
        Assert.Equal(CouponFailureReason.RedemptionLimitReached, result.FailureReason);
    }

    [Fact]
    public void Validate_ChecksExpiryBeforeMinimumSpend_WhenBothApply()
    {
        // Order matters for an accurate "why did this fail" answer — an
        // expired coupon should say so, not claim the cart wasn't big enough.
        var coupon = CreatePercentageCoupon(minimumSpendAmount: 1000m, expiresAtUtc: Now.AddDays(-1));

        var result = coupon.Validate(cartSubtotal: 1m, Now);

        Assert.Equal(CouponFailureReason.Expired, result.FailureReason);
    }
}
