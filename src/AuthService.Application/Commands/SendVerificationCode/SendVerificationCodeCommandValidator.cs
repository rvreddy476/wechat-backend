using AuthService.Domain.Constants;
using FluentValidation;

namespace AuthService.Application.Commands.SendVerificationCode;

public class SendVerificationCodeCommandValidator : AbstractValidator<SendVerificationCodeCommand>
{
    public SendVerificationCodeCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(ValidationMessages.UserIdRequired);

        RuleFor(x => x.Target)
            .NotEmpty().WithMessage(ValidationMessages.VerificationTargetRequired);

        RuleFor(x => x.VerificationType)
            .NotEmpty().WithMessage(ValidationMessages.VerificationTypeRequired)
            .Must(type => type == "Email" || type == "Phone")
            .WithMessage(ValidationMessages.VerificationTypeInvalid);

        // Validate email format when type is Email
        RuleFor(x => x.Target)
            .EmailAddress().WithMessage(ValidationMessages.EmailInvalid)
            .When(x => x.VerificationType == "Email");

        // Validate phone format when type is Phone
        RuleFor(x => x.Target)
            .Matches(@"^\+[1-9]\d{1,14}$").WithMessage(ValidationMessages.PhoneNumberInvalid)
            .When(x => x.VerificationType == "Phone");
    }
}
