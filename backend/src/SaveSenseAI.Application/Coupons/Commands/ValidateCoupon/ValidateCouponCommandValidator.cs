using FluentValidation;

namespace SaveSenseAI.Application.Coupons.Commands.ValidateCoupon;

public sealed class ValidateCouponCommandValidator : AbstractValidator<ValidateCouponCommand>
{
    public ValidateCouponCommandValidator()
    {
        RuleFor(x => x.StoreId).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(64);
        RuleFor(x => x.CartSubtotal).GreaterThanOrEqualTo(0);
    }
}
