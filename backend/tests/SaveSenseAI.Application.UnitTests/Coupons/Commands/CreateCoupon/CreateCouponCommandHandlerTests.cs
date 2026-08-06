using Microsoft.EntityFrameworkCore;
using SaveSenseAI.Application.Common.Exceptions;
using SaveSenseAI.Application.Coupons.Commands.CreateCoupon;
using SaveSenseAI.Domain.Entities;
using SaveSenseAI.Infrastructure.Persistence;

namespace SaveSenseAI.Application.UnitTests.Coupons.Commands.CreateCoupon;

public class CreateCouponCommandHandlerTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Handle_ExistingStore_PersistsCouponAndReturnsItsId()
    {
        await using var context = CreateContext();
        var store = Store.Create("Nike", "nike");
        context.Stores.Add(store);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new CreateCouponCommandHandler(context);
        var command = new CreateCouponCommand(store.Id, "SAVE20", "20% off", DiscountType.Percentage, 20m, null, null, null);

        var id = await handler.Handle(command, CancellationToken.None);

        var coupon = await context.Coupons.SingleAsync(c => c.Id == id);
        Assert.Equal("SAVE20", coupon.Code);
        Assert.Equal(store.Id, coupon.StoreId);
    }

    [Fact]
    public async Task Handle_NonExistentStore_ThrowsNotFoundException()
    {
        await using var context = CreateContext();
        var handler = new CreateCouponCommandHandler(context);
        var command = new CreateCouponCommand(Guid.NewGuid(), "SAVE20", "20% off", DiscountType.Percentage, 20m, null, null, null);

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }
}
