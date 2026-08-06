using FluentValidation;
using SaveSenseAI.Domain.Entities;

namespace SaveSenseAI.Application.Coupons.Commands.CreateCoupon;

public sealed class CreateCouponCommandValidator : AbstractValidator<CreateCouponCommand>
{
    public CreateCouponCommandValidator()
    {
        RuleFor(x => x.StoreId).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.DiscountType).IsInEnum();

        RuleFor(x => x.DiscountValue).GreaterThan(0);
        RuleFor(x => x.DiscountValue)
            .LessThanOrEqualTo(100)
            .When(x => x.DiscountType == DiscountType.Percentage)
            .WithMessage("A percentage discount cannot exceed 100.");

        RuleFor(x => x.MinimumSpendAmount).GreaterThanOrEqualTo(0).When(x => x.MinimumSpendAmount.HasValue);
        RuleFor(x => x.MaxRedemptions).GreaterThan(0).When(x => x.MaxRedemptions.HasValue);
    }
}
