using Microsoft.EntityFrameworkCore;
using SaveSenseAI.Application.Coupons.Commands.ValidateCoupon;
using SaveSenseAI.Application.UnitTests.Common;
using SaveSenseAI.Domain.Entities;
using SaveSenseAI.Infrastructure.Persistence;

namespace SaveSenseAI.Application.UnitTests.Coupons.Commands.ValidateCoupon;

public class ValidateCouponCommandHandlerTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static async Task<(ApplicationDbContext Context, Store Store, Coupon Coupon)> SeedStoreWithCouponAsync(
        DateTimeOffset now, decimal discountValue = 20)
    {
        var context = CreateContext();
        var store = Store.Create("Nike", "nike");
        var coupon = Coupon.Create(store.Id, "SAVE20", "20% off", DiscountType.Percentage, discountValue, null, null, null);
        context.Stores.Add(store);
        context.Coupons.Add(coupon);
        await context.SaveChangesAsync(CancellationToken.None);
        return (context, store, coupon);
    }

    [Fact]
    public async Task Handle_ValidCoupon_ReturnsSuccessAndIncrementsRedemptionCount()
    {
        var now = DateTimeOffset.UtcNow;
        var (context, store, coupon) = await SeedStoreWithCouponAsync(now);
        var handler = new ValidateCouponCommandHandler(context, new FixedDateTime(now), new FakeCurrentUserService());

        var result = await handler.Handle(new ValidateCouponCommand(store.Id, "SAVE20", 100m), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(20m, result.DiscountAmount);

        var persistedCoupon = await context.Coupons.SingleAsync(c => c.Id == coupon.Id);
        Assert.Equal(1, persistedCoupon.RedemptionCount);
    }

    [Fact]
    public async Task Handle_CodeIsCaseInsensitiveAndTrimmed()
    {
        var now = DateTimeOffset.UtcNow;
        var (context, store, _) = await SeedStoreWithCouponAsync(now);
        var handler = new ValidateCouponCommandHandler(context, new FixedDateTime(now), new FakeCurrentUserService());

        var result = await handler.Handle(new ValidateCouponCommand(store.Id, "  save20  ", 100m), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_UnknownCode_ReturnsNotFoundAndPersistsAttemptWithNullCoupon()
    {
        var now = DateTimeOffset.UtcNow;
        var (context, store, _) = await SeedStoreWithCouponAsync(now);
        var handler = new ValidateCouponCommandHandler(context, new FixedDateTime(now), new FakeCurrentUserService());

        var result = await handler.Handle(new ValidateCouponCommand(store.Id, "NOTREAL", 100m), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(CouponFailureReason.NotFound, result.FailureReason);

        var attempt = await context.CouponValidationAttempts.SingleAsync(a => a.AttemptedCode == "NOTREAL");
        Assert.Null(attempt.CouponId);
        Assert.False(attempt.IsSuccess);
    }

    [Fact]
    public async Task Handle_FailedValidation_DoesNotIncrementRedemptionCount()
    {
        var now = DateTimeOffset.UtcNow;
        var (context, store, coupon) = await SeedStoreWithCouponAsync(now);
        var handler = new ValidateCouponCommandHandler(context, new FixedDateTime(now), new FakeCurrentUserService());

        // Cart of 0 with no minimum spend still succeeds, so force a failure
        // via an unknown code instead — simplest reliable failure path here.
        await handler.Handle(new ValidateCouponCommand(store.Id, "WRONGCODE", 100m), CancellationToken.None);

        var persistedCoupon = await context.Coupons.SingleAsync(c => c.Id == coupon.Id);
        Assert.Equal(0, persistedCoupon.RedemptionCount);
    }

    [Fact]
    public async Task Handle_AnonymousCaller_PersistsAttemptWithNullUserId()
    {
        var now = DateTimeOffset.UtcNow;
        var (context, store, _) = await SeedStoreWithCouponAsync(now);
        var handler = new ValidateCouponCommandHandler(context, new FixedDateTime(now), new FakeCurrentUserService(userId: null));

        await handler.Handle(new ValidateCouponCommand(store.Id, "SAVE20", 100m), CancellationToken.None);

        var attempt = await context.CouponValidationAttempts.SingleAsync();
        Assert.Null(attempt.UserId);
    }

    [Fact]
    public async Task Handle_AuthenticatedCaller_PersistsAttemptWithUserId()
    {
        var now = DateTimeOffset.UtcNow;
        var (context, store, _) = await SeedStoreWithCouponAsync(now);
        var userId = Guid.NewGuid();
        var handler = new ValidateCouponCommandHandler(context, new FixedDateTime(now), new FakeCurrentUserService(userId));

        await handler.Handle(new ValidateCouponCommand(store.Id, "SAVE20", 100m), CancellationToken.None);

        var attempt = await context.CouponValidationAttempts.SingleAsync();
        Assert.Equal(userId, attempt.UserId);
    }
}
