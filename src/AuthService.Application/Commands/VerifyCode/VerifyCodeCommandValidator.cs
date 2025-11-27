using AuthService.Domain.Constants;
using FluentValidation;

namespace AuthService.Application.Commands.VerifyCode;

public class VerifyCodeCommandValidator : AbstractValidator<VerifyCodeCommand>
{
    public VerifyCodeCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(ValidationMessages.UserIdRequired);

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage(ValidationMessages.VerificationCodeRequired)
            .Length(AuthConstants.VerificationCodeLength).WithMessage(ValidationMessages.VerificationCodeLength)
            .Matches(@"^\d{6}$").WithMessage(ValidationMessages.VerificationCodeFormat);

        RuleFor(x => x.VerificationType)
            .NotEmpty().WithMessage(ValidationMessages.VerificationTypeRequired)
            .Must(type => type == "Email" || type == "Phone")
            .WithMessage(ValidationMessages.VerificationTypeInvalid);
    }
}
