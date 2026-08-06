using MediatR;

namespace SaveSenseAI.Application.Coupons.Commands.CreateStore;

public sealed record CreateStoreCommand(string Name, string Slug) : IRequest<Guid>;
