using MediatR;
using SaveSenseAI.Application.Coupons.Commands.CreateCoupon;
using SaveSenseAI.Application.Coupons.Commands.CreateStore;
using SaveSenseAI.Application.Coupons.Commands.ValidateCoupon;
using SaveSenseAI.Application.Coupons.Queries.GetCouponsForStore;
using SaveSenseAI.Domain.Entities;

namespace SaveSenseAI.API.Endpoints;

public static class CouponEndpoints
{
    public static IEndpointRouteBuilder MapCouponEndpoints(this IEndpointRouteBuilder app)
    {
        var stores = app.MapGroup("/api/stores").WithTags("Coupons");
        var coupons = app.MapGroup("/api/coupons").WithTags("Coupons");

        stores.MapPost("/", async (CreateStoreCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            var id = await sender.Send(command, cancellationToken);
            return Results.Created($"/api/stores/{id}", new { id });
        })
        .RequireAuthorization();

        stores.MapGet("/{storeId:guid}/coupons", async (Guid storeId, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetCouponsForStoreQuery(storeId), cancellationToken);
            return Results.Ok(result);
        });

        stores.MapPost("/{storeId:guid}/coupons", async (
            Guid storeId,
            CreateCouponRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateCouponCommand(
                storeId,
                request.Code,
                request.Description,
                request.DiscountType,
                request.DiscountValue,
                request.MinimumSpendAmount,
                request.ExpiresAtUtc,
                request.MaxRedemptions);

            var id = await sender.Send(command, cancellationToken);
            return Results.Created($"/api/stores/{storeId}/coupons/{id}", new { id });
        })
        .RequireAuthorization();

        coupons.MapPost("/validate", async (ValidateCouponCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return Results.Ok(result);
        });

        return app;
    }

    private sealed record CreateCouponRequest(
        string Code,
        string Description,
        DiscountType DiscountType,
        decimal DiscountValue,
        decimal? MinimumSpendAmount,
        DateTimeOffset? ExpiresAtUtc,
        int? MaxRedemptions);
}
