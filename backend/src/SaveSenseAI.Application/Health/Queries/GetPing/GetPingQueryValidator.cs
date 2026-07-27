using FluentValidation;

namespace SaveSenseAI.Application.Health.Queries.GetPing;

public sealed class GetPingQueryValidator : AbstractValidator<GetPingQuery>
{
    public GetPingQueryValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100)
            .WithMessage("Name must not exceed 100 characters.");
    }
}
