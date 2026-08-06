using MediatR;
using SaveSenseAI.Application.Common.Interfaces;
using SaveSenseAI.Domain.Entities;

namespace SaveSenseAI.Application.Coupons.Commands.CreateStore;

public sealed class CreateStoreCommandHandler(IApplicationDbContext dbContext) : IRequestHandler<CreateStoreCommand, Guid>
{
    public async Task<Guid> Handle(CreateStoreCommand request, CancellationToken cancellationToken)
    {
        var store = Store.Create(request.Name, request.Slug);

        dbContext.Stores.Add(store);
        await dbContext.SaveChangesAsync(cancellationToken);

        return store.Id;
    }
}
