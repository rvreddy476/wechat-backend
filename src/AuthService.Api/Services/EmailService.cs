namespace AuthService.Api.Services;

/// <summary>
/// Email service implementation
/// TODO: Replace with actual email provider (SendGrid, AWS SES, etc.)
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _configuration;

    public EmailService(ILogger<EmailService> logger, IConfiguration _configuration)
    {
        _logger = logger;
        this._configuration = _configuration;
    }

    public async Task<bool> SendVerificationCodeAsync(string email, string code, string firstName)
    {
        try
        {
            // TODO: Implement actual email sending logic
            // For now, log the verification code
            _logger.LogInformation(
                "=== EMAIL VERIFICATION CODE ===\n" +
                "To: {Email}\n" +
                "Subject: Verify Your WeChat Account\n" +
                "---\n" +
                "Hi {FirstName},\n\n" +
                "Your verification code is: {Code}\n\n" +
                "This code will expire in 10 minutes.\n\n" +
                "If you didn't request this code, please ignore this email.\n" +
                "===============================",
                email, firstName, code);

            // Simulate email sending delay
            await Task.Delay(100);

            // In production, use a real email provider:
            /*
            var apiKey = _configuration["SendGrid:ApiKey"];
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("noreply@wechat.com", "WeChat");
            var to = new EmailAddress(email, firstName);
            var subject = "Verify Your WeChat Account";
            var htmlContent = $@"
                <h2>Hi {firstName},</h2>
                <p>Your verification code is:</p>
                <h1 style='color: #007bff; letter-spacing: 5px;'>{code}</h1>
                <p>This code will expire in 10 minutes.</p>
                <p>If you didn't request this code, please ignore this email.</p>
            ";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);
            var response = await client.SendEmailAsync(msg);
            return response.IsSuccessStatusCode;
            */

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending verification email to {Email}", email);
            return false;
        }
    }

    public async Task<bool> SendPasswordResetEmailAsync(string email, string resetToken, string firstName)
    {
        try
        {
            _logger.LogInformation(
                "=== PASSWORD RESET EMAIL ===\n" +
                "To: {Email}\n" +
                "Subject: Reset Your Password\n" +
                "---\n" +
                "Hi {FirstName},\n\n" +
                "Click the link below to reset your password:\n" +
                "https://wechat.com/reset-password?token={Token}\n\n" +
                "This link will expire in 1 hour.\n\n" +
                "If you didn't request this, please ignore this email.\n" +
                "===========================",
                email, firstName, resetToken);

            await Task.Delay(100);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending password reset email to {Email}", email);
            return false;
        }
    }

    public async Task<bool> SendWelcomeEmailAsync(string email, string firstName)
    {
        try
        {
            _logger.LogInformation(
                "=== WELCOME EMAIL ===\n" +
                "To: {Email}\n" +
                "Subject: Welcome to WeChat!\n" +
                "---\n" +
                "Hi {FirstName},\n\n" +
                "Welcome to WeChat! We're excited to have you.\n\n" +
                "Get started by:\n" +
                "1. Completing your profile\n" +
                "2. Finding friends\n" +
                "3. Sharing your first post\n\n" +
                "Enjoy!\n" +
                "====================",
                email, firstName);

            await Task.Delay(100);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending welcome email to {Email}", email);
            return false;
        }
    }
}
