namespace AuthService.Domain.Constants;

public static class ValidationMessages
{
    // User Registration
    public const string FirstNameRequired = "First name is required";
    public const string FirstNameMinLength = "First name must be at least 2 characters";
    public const string FirstNameMaxLength = "First name cannot exceed 50 characters";

    public const string LastNameRequired = "Last name is required";
    public const string LastNameMinLength = "Last name must be at least 2 characters";
    public const string LastNameMaxLength = "Last name cannot exceed 50 characters";

    public const string EmailRequired = "Email is required";
    public const string EmailInvalid = "Email format is invalid";
    public const string EmailMaxLength = "Email cannot exceed 255 characters";

    public const string PhoneNumberRequired = "Phone number is required";
    public const string PhoneNumberInvalid = "Phone number format is invalid (use E.164 format: +1234567890)";

    public const string PasswordRequired = "Password is required";
    public const string PasswordMinLength = "Password must be at least 8 characters";
    public const string PasswordMaxLength = "Password cannot exceed 128 characters";
    public const string PasswordComplexity = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character";

    public const string GenderRequired = "Gender is required";
    public const string GenderInvalid = "Invalid gender value. Must be: Male, Female, Other, or PreferNotToSay";

    public const string DateOfBirthRequired = "Date of birth is required";
    public const string DateOfBirthInvalid = "Date of birth cannot be in the future";
    public const string DateOfBirthMinAge = "You must be at least 13 years old to register";

    public const string HandlerMinLength = "Handler must be at least 3 characters";
    public const string HandlerMaxLength = "Handler cannot exceed 30 characters";
    public const string HandlerInvalid = "Handler can only contain letters, numbers, and underscores";

    // User Login
    public const string UsernameOrEmailRequired = "Username or email is required";
    public const string PasswordRequiredForLogin = "Password is required";

    // Verification Code
    public const string VerificationCodeRequired = "Verification code is required";
    public const string VerificationCodeLength = "Verification code must be 6 digits";
    public const string VerificationCodeFormat = "Verification code must contain only digits";
    public const string VerificationTypeRequired = "Verification type is required";
    public const string VerificationTypeInvalid = "Verification type must be either 'Email' or 'Phone'";
    public const string VerificationTargetRequired = "Email or phone number is required";

    // Token Validation
    public const string RefreshTokenRequired = "Refresh token is required";
    public const string ResetTokenRequired = "Reset token is required";

    // Common
    public const string UserIdRequired = "User ID is required";
    public const string InvalidGuid = "Invalid GUID format";
}
