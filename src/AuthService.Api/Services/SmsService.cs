namespace AuthService.Api.Services;

/// <summary>
/// SMS service implementation
/// TODO: Replace with actual SMS provider (Twilio, AWS SNS, etc.)
/// </summary>
public class SmsService : ISmsService
{
    private readonly ILogger<SmsService> _logger;
    private readonly IConfiguration _configuration;

    public SmsService(ILogger<SmsService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<bool> SendVerificationCodeAsync(string phoneNumber, string code)
    {
        try
        {
            // TODO: Implement actual SMS sending logic
            // For now, log the verification code
            _logger.LogInformation(
                "=== SMS VERIFICATION CODE ===\n" +
                "To: {PhoneNumber}\n" +
                "---\n" +
                "Your WeChat verification code is: {Code}\n\n" +
                "This code will expire in 10 minutes.\n" +
                "=============================",
                phoneNumber, code);

            // Simulate SMS sending delay
            await Task.Delay(100);

            // In production, use a real SMS provider:
            /*
            var accountSid = _configuration["Twilio:AccountSid"];
            var authToken = _configuration["Twilio:AuthToken"];
            var fromNumber = _configuration["Twilio:PhoneNumber"];

            TwilioClient.Init(accountSid, authToken);

            var message = await MessageResource.CreateAsync(
                body: $"Your WeChat verification code is: {code}. This code will expire in 10 minutes.",
                from: new PhoneNumber(fromNumber),
                to: new PhoneNumber(phoneNumber)
            );

            return message.Status != MessageResource.StatusEnum.Failed;
            */

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending verification SMS to {PhoneNumber}", phoneNumber);
            return false;
        }
    }

    public async Task<bool> SendPasswordResetCodeAsync(string phoneNumber, string code)
    {
        try
        {
            _logger.LogInformation(
                "=== SMS PASSWORD RESET CODE ===\n" +
                "To: {PhoneNumber}\n" +
                "---\n" +
                "Your WeChat password reset code is: {Code}\n\n" +
                "This code will expire in 10 minutes.\n" +
                "===============================",
                phoneNumber, code);

            await Task.Delay(100);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending password reset SMS to {PhoneNumber}", phoneNumber);
            return false;
        }
    }
}
