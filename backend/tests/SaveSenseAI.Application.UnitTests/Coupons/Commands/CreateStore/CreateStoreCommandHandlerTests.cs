using Microsoft.EntityFrameworkCore;
using SaveSenseAI.Application.Coupons.Commands.CreateStore;
using SaveSenseAI.Infrastructure.Persistence;

namespace SaveSenseAI.Application.UnitTests.Coupons.Commands.CreateStore;

public class CreateStoreCommandHandlerTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Handle_ValidCommand_PersistsStoreAndReturnsItsId()
    {
        await using var context = CreateContext();
        var handler = new CreateStoreCommandHandler(context);

        var id = await handler.Handle(new CreateStoreCommand("Nike", "nike"), CancellationToken.None);

        var store = await context.Stores.SingleAsync(s => s.Id == id);
        Assert.Equal("Nike", store.Name);
        Assert.Equal("nike", store.Slug);
    }
}
