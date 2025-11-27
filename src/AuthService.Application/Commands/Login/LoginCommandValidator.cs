using AuthService.Domain.Constants;
using FluentValidation;

namespace AuthService.Application.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.UsernameOrEmail)
            .NotEmpty().WithMessage(ValidationMessages.UsernameOrEmailRequired);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(ValidationMessages.PasswordRequiredForLogin);
    }
}
