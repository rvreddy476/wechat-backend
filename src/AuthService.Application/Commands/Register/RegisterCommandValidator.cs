using AuthService.Domain.Constants;
using AuthService.Domain.Enums;
using FluentValidation;

namespace AuthService.Application.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        // First Name Validation
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage(ValidationMessages.FirstNameRequired)
            .MinimumLength(AuthConstants.MinNameLength).WithMessage(ValidationMessages.FirstNameMinLength)
            .MaximumLength(AuthConstants.MaxFirstNameLength).WithMessage(ValidationMessages.FirstNameMaxLength);

        // Last Name Validation
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage(ValidationMessages.LastNameRequired)
            .MinimumLength(AuthConstants.MinNameLength).WithMessage(ValidationMessages.LastNameMinLength)
            .MaximumLength(AuthConstants.MaxLastNameLength).WithMessage(ValidationMessages.LastNameMaxLength);

        // Email Validation
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(ValidationMessages.EmailRequired)
            .EmailAddress().WithMessage(ValidationMessages.EmailInvalid)
            .MaximumLength(AuthConstants.MaxEmailLength).WithMessage(ValidationMessages.EmailMaxLength);

        // Phone Number Validation (E.164 format: +1234567890)
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage(ValidationMessages.PhoneNumberRequired)
            .Matches(@"^\+[1-9]\d{1,14}$").WithMessage(ValidationMessages.PhoneNumberInvalid);

        // Password Validation
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(ValidationMessages.PasswordRequired)
            .MinimumLength(AuthConstants.MinPasswordLength).WithMessage(ValidationMessages.PasswordMinLength)
            .MaximumLength(AuthConstants.MaxPasswordLength).WithMessage(ValidationMessages.PasswordMaxLength)
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]")
            .WithMessage(ValidationMessages.PasswordComplexity);

        // Gender Validation
        RuleFor(x => x.Gender)
            .NotEmpty().WithMessage(ValidationMessages.GenderRequired)
            .Must(BeValidGender).WithMessage(ValidationMessages.GenderInvalid);

        // Date of Birth Validation
        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage(ValidationMessages.DateOfBirthRequired)
            .LessThanOrEqualTo(DateTime.UtcNow.Date).WithMessage(ValidationMessages.DateOfBirthInvalid)
            .Must(BeAtLeastMinimumAge).WithMessage(ValidationMessages.DateOfBirthMinAge);

        // Handler Validation (Optional)
        RuleFor(x => x.Handler)
            .MinimumLength(AuthConstants.MinHandlerLength).WithMessage(ValidationMessages.HandlerMinLength)
            .MaximumLength(AuthConstants.MaxHandlerLength).WithMessage(ValidationMessages.HandlerMaxLength)
            .Matches(@"^[a-zA-Z0-9_]+$").WithMessage(ValidationMessages.HandlerInvalid)
            .When(x => !string.IsNullOrWhiteSpace(x.Handler));
    }

    private bool BeValidGender(string gender)
    {
        return Enum.TryParse<GenderType>(gender, ignoreCase: true, out _);
    }

    private bool BeAtLeastMinimumAge(DateTime dateOfBirth)
    {
        var today = DateTime.UtcNow.Date;
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age))
        {
            age--;
        }
        return age >= AuthConstants.MinimumAgeYears;
    }
}
