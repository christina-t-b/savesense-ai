using MediatR;
using Microsoft.EntityFrameworkCore;
using SaveSenseAI.Application.Common.Exceptions;
using SaveSenseAI.Application.Common.Interfaces;
using SaveSenseAI.Domain.Entities;

namespace SaveSenseAI.Application.Coupons.Commands.CreateCoupon;

public sealed class CreateCouponCommandHandler(IApplicationDbContext dbContext) : IRequestHandler<CreateCouponCommand, Guid>
{
    public async Task<Guid> Handle(CreateCouponCommand request, CancellationToken cancellationToken)
    {
        var storeExists = await dbContext.Stores.AnyAsync(s => s.Id == request.StoreId, cancellationToken);
        if (!storeExists)
        {
            throw new NotFoundException(nameof(Store), request.StoreId);
        }

        var coupon = Coupon.Create(
            request.StoreId,
            request.Code,
            request.Description,
            request.DiscountType,
            request.DiscountValue,
            request.MinimumSpendAmount,
            request.ExpiresAtUtc,
            request.MaxRedemptions);

        dbContext.Coupons.Add(coupon);
        await dbContext.SaveChangesAsync(cancellationToken);

        return coupon.Id;
    }
}
