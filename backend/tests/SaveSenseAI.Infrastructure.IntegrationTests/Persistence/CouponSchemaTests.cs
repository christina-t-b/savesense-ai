using Microsoft.EntityFrameworkCore;
using SaveSenseAI.Domain.Entities;

namespace SaveSenseAI.Infrastructure.IntegrationTests.Persistence;

public class CouponSchemaTests(PostgresFixture fixture) : IClassFixture<PostgresFixture>
{
    [Fact]
    public async Task Stores_DuplicateSlug_ViolatesUniqueConstraint()
    {
        await using var context = fixture.CreateContext();
        var slug = $"store-{Guid.NewGuid()}";
        context.Stores.Add(Store.Create("First", slug));
        await context.SaveChangesAsync(CancellationToken.None);

        await using var secondContext = fixture.CreateContext();
        secondContext.Stores.Add(Store.Create("Second", slug));

        await Assert.ThrowsAsync<DbUpdateException>(() => secondContext.SaveChangesAsync(CancellationToken.None));
    }

    [Fact]
    public async Task Coupons_DuplicateCodeForSameStore_ViolatesUniqueConstraint()
    {
        await using var context = fixture.CreateContext();
        var store = Store.Create("First", $"store-{Guid.NewGuid()}");
        context.Stores.Add(store);
        await context.SaveChangesAsync(CancellationToken.None);

        context.Coupons.Add(Coupon.Create(store.Id, "SAVE20", "desc", DiscountType.Percentage, 20m, null, null, null));
        await context.SaveChangesAsync(CancellationToken.None);

        await using var secondContext = fixture.CreateContext();
        secondContext.Coupons.Add(Coupon.Create(store.Id, "SAVE20", "duplicate", DiscountType.Percentage, 10m, null, null, null));

        await Assert.ThrowsAsync<DbUpdateException>(() => secondContext.SaveChangesAsync(CancellationToken.None));
    }

    [Fact]
    public async Task Coupons_SameCodeAtDifferentStores_IsAllowed()
    {
        await using var context = fixture.CreateContext();
        var storeA = Store.Create("Store A", $"store-a-{Guid.NewGuid()}");
        var storeB = Store.Create("Store B", $"store-b-{Guid.NewGuid()}");
        context.Stores.AddRange(storeA, storeB);
        context.Coupons.Add(Coupon.Create(storeA.Id, "SAVE20", "desc", DiscountType.Percentage, 20m, null, null, null));
        context.Coupons.Add(Coupon.Create(storeB.Id, "SAVE20", "desc", DiscountType.Percentage, 20m, null, null, null));

        // Two different retailers both running a "SAVE20" code shouldn't collide.
        await context.SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task DeletingStore_CascadesToCouponsAndValidationAttempts()
    {
        await using var context = fixture.CreateContext();
        var store = Store.Create("Nike", $"nike-{Guid.NewGuid()}");
        context.Stores.Add(store);
        var coupon = Coupon.Create(store.Id, "SAVE20", "desc", DiscountType.Percentage, 20m, null, null, null);
        context.Coupons.Add(coupon);
        await context.SaveChangesAsync(CancellationToken.None);

        var outcome = CouponValidationOutcome.Success(20m);
        var attempt = CouponValidationAttempt.Record(store.Id, coupon.Id, "SAVE20", 100m, null, outcome, DateTimeOffset.UtcNow);
        context.CouponValidationAttempts.Add(attempt);
        await context.SaveChangesAsync(CancellationToken.None);

        await using var deleteContext = fixture.CreateContext();
        var storeToDelete = await deleteContext.Stores.SingleAsync(s => s.Id == store.Id);
        deleteContext.Stores.Remove(storeToDelete);
        await deleteContext.SaveChangesAsync(CancellationToken.None);

        await using var verifyContext = fixture.CreateContext();
        Assert.False(await verifyContext.Coupons.AnyAsync(c => c.Id == coupon.Id));
        Assert.False(await verifyContext.CouponValidationAttempts.AnyAsync(a => a.Id == attempt.Id));
    }

    [Fact]
    public async Task DeletingCoupon_SetsValidationAttemptCouponIdToNull_PreservingTheRecord()
    {
        await using var context = fixture.CreateContext();
        var store = Store.Create("Nike", $"nike-{Guid.NewGuid()}");
        context.Stores.Add(store);
        var coupon = Coupon.Create(store.Id, "SAVE20", "desc", DiscountType.Percentage, 20m, null, null, null);
        context.Coupons.Add(coupon);
        await context.SaveChangesAsync(CancellationToken.None);

        var outcome = CouponValidationOutcome.Success(20m);
        var attempt = CouponValidationAttempt.Record(store.Id, coupon.Id, "SAVE20", 100m, null, outcome, DateTimeOffset.UtcNow);
        context.CouponValidationAttempts.Add(attempt);
        await context.SaveChangesAsync(CancellationToken.None);

        await using var deleteContext = fixture.CreateContext();
        var couponToDelete = await deleteContext.Coupons.SingleAsync(c => c.Id == coupon.Id);
        deleteContext.Coupons.Remove(couponToDelete);
        await deleteContext.SaveChangesAsync(CancellationToken.None);

        await using var verifyContext = fixture.CreateContext();
        var persistedAttempt = await verifyContext.CouponValidationAttempts.SingleAsync(a => a.Id == attempt.Id);
        Assert.Null(persistedAttempt.CouponId);
    }
}
