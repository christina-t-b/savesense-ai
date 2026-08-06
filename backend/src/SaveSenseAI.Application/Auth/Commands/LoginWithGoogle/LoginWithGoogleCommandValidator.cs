using FluentValidation;

namespace SaveSenseAI.Application.Auth.Commands.LoginWithGoogle;

public sealed class LoginWithGoogleCommandValidator : AbstractValidator<LoginWithGoogleCommand>
{
    public LoginWithGoogleCommandValidator()
    {
        RuleFor(x => x.GoogleSubjectId).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}
