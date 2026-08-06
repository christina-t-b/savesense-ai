using Microsoft.EntityFrameworkCore;
using SaveSenseAI.Application.Coupons.Queries.GetCouponsForStore;
using SaveSenseAI.Application.UnitTests.Common;
using SaveSenseAI.Domain.Entities;
using SaveSenseAI.Infrastructure.Persistence;

namespace SaveSenseAI.Application.UnitTests.Coupons.Queries.GetCouponsForStore;

public class GetCouponsForStoreQueryHandlerTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Handle_ExcludesExpiredAndRedemptionExhaustedCoupons()
    {
        await using var context = CreateContext();
        var now = DateTimeOffset.UtcNow;
        var store = Store.Create("Nike", "nike");
        context.Stores.Add(store);

        var validCoupon = Coupon.Create(store.Id, "VALID", "still good", DiscountType.Percentage, 10m, null, null, null);

        var expiredCoupon = Coupon.Create(store.Id, "EXPIRED", "expired", DiscountType.Percentage, 10m, null, now.AddDays(-1), null);

        var exhaustedCoupon = Coupon.Create(store.Id, "EXHAUSTED", "maxed out", DiscountType.Percentage, 10m, null, null, 1);
        exhaustedCoupon.RecordRedemption();

        context.Coupons.AddRange(validCoupon, expiredCoupon, exhaustedCoupon);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetCouponsForStoreQueryHandler(context, new FixedDateTime(now));
        var results = await handler.Handle(new GetCouponsForStoreQuery(store.Id), CancellationToken.None);

        var code = Assert.Single(results).Code;
        Assert.Equal("VALID", code);
    }

    [Fact]
    public async Task Handle_OrdersByDiscountValueDescending()
    {
        await using var context = CreateContext();
        var now = DateTimeOffset.UtcNow;
        var store = Store.Create("Nike", "nike");
        context.Stores.Add(store);
        context.Coupons.AddRange(
            Coupon.Create(store.Id, "SMALL", "10% off", DiscountType.Percentage, 10m, null, null, null),
            Coupon.Create(store.Id, "BIG", "30% off", DiscountType.Percentage, 30m, null, null, null));
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetCouponsForStoreQueryHandler(context, new FixedDateTime(now));
        var results = await handler.Handle(new GetCouponsForStoreQuery(store.Id), CancellationToken.None);

        Assert.Equal("BIG", results[0].Code);
        Assert.Equal("SMALL", results[1].Code);
    }
}
