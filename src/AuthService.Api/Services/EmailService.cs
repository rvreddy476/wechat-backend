using SendGrid;
using SendGrid.Helpers.Mail;

namespace AuthService.Api.Services;

/// <summary>
/// Email service implementation using SendGrid
/// Falls back to console logging if SendGrid is not configured
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _configuration;
    private readonly bool _isConfigured;
    private readonly string? _apiKey;
    private readonly string? _fromEmail;
    private readonly string? _fromName;

    public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;

        // Check if SendGrid is configured
        _apiKey = _configuration["SendGrid:ApiKey"];
        _fromEmail = _configuration["SendGrid:FromEmail"] ?? "noreply@wechat.com";
        _fromName = _configuration["SendGrid:FromName"] ?? "WeChat";

        _isConfigured = !string.IsNullOrEmpty(_apiKey) && _apiKey != "your-sendgrid-api-key-here";

        if (!_isConfigured)
        {
            _logger.LogWarning(
                "SendGrid is not configured. Email sending will fall back to console logging. " +
                "Please configure SendGrid:ApiKey in appsettings.json for production use.");
        }
        else
        {
            _logger.LogInformation("SendGrid email service initialized successfully");
        }
    }

    public async Task<bool> SendVerificationCodeAsync(string email, string code, string firstName)
    {
        try
        {
            if (_isConfigured)
            {
                return await SendViaSendGrid(
                    email,
                    firstName,
                    "Verify Your WeChat Account",
                    BuildVerificationEmailHtml(firstName, code),
                    BuildVerificationEmailText(firstName, code)
                );
            }
            else
            {
                return LogEmailToConsole(email, firstName, "Verify Your WeChat Account", code);
            }
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
            var resetLink = $"{_configuration["AppUrl"]}/reset-password?token={resetToken}";

            if (_isConfigured)
            {
                return await SendViaSendGrid(
                    email,
                    firstName,
                    "Reset Your WeChat Password",
                    BuildPasswordResetEmailHtml(firstName, resetLink),
                    BuildPasswordResetEmailText(firstName, resetLink)
                );
            }
            else
            {
                _logger.LogInformation(
                    "=== PASSWORD RESET EMAIL ===\n" +
                    "To: {Email}\n" +
                    "Subject: Reset Your Password\n" +
                    "---\n" +
                    "Hi {FirstName},\n\n" +
                    "Click the link below to reset your password:\n" +
                    "{ResetLink}\n\n" +
                    "This link will expire in 1 hour.\n\n" +
                    "If you didn't request this, please ignore this email.\n" +
                    "===========================",
                    email, firstName, resetLink);
                return true;
            }
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
            if (_isConfigured)
            {
                return await SendViaSendGrid(
                    email,
                    firstName,
                    "Welcome to WeChat!",
                    BuildWelcomeEmailHtml(firstName),
                    BuildWelcomeEmailText(firstName)
                );
            }
            else
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
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending welcome email to {Email}", email);
            return false;
        }
    }

    #region Private Helper Methods

    private async Task<bool> SendViaSendGrid(
        string toEmail,
        string toName,
        string subject,
        string htmlContent,
        string plainTextContent)
    {
        try
        {
            var client = new SendGridClient(_apiKey);
            var from = new EmailAddress(_fromEmail, _fromName);
            var to = new EmailAddress(toEmail, toName);

            var msg = MailHelper.CreateSingleEmail(
                from,
                to,
                subject,
                plainTextContent,
                htmlContent
            );

            var response = await client.SendEmailAsync(msg);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "Email sent successfully via SendGrid to {Email}. Subject: {Subject}",
                    toEmail, subject);
                return true;
            }
            else
            {
                var body = await response.Body.ReadAsStringAsync();
                _logger.LogError(
                    "SendGrid failed to send email to {Email}. Status: {Status}, Body: {Body}",
                    toEmail, response.StatusCode, body);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception sending email via SendGrid to {Email}", toEmail);
            return false;
        }
    }

    private bool LogEmailToConsole(string email, string firstName, string subject, string code)
    {
        _logger.LogInformation(
            "=== EMAIL (CONSOLE MODE) ===\n" +
            "To: {Email}\n" +
            "Name: {FirstName}\n" +
            "Subject: {Subject}\n" +
            "---\n" +
            "Verification Code: {Code}\n\n" +
            "This code will expire in 10 minutes.\n" +
            "============================",
            email, firstName, subject, code);
        return true;
    }

    private string BuildVerificationEmailHtml(string firstName, string code)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
        <h1 style='color: white; margin: 0; font-size: 28px;'>WeChat</h1>
    </div>

    <div style='background: #ffffff; padding: 40px; border: 1px solid #e0e0e0; border-top: none; border-radius: 0 0 10px 10px;'>
        <h2 style='color: #333; margin-top: 0;'>Hi {firstName},</h2>

        <p style='font-size: 16px; color: #555;'>
            Thank you for registering with WeChat! To complete your registration, please verify your email address using the code below:
        </p>

        <div style='background: #f8f9fa; padding: 25px; text-align: center; border-radius: 8px; margin: 30px 0;'>
            <p style='margin: 0 0 10px 0; color: #666; font-size: 14px; text-transform: uppercase; letter-spacing: 1px;'>Your Verification Code</p>
            <h1 style='color: #667eea; letter-spacing: 8px; margin: 0; font-size: 42px; font-weight: bold;'>{code}</h1>
        </div>

        <p style='font-size: 14px; color: #777;'>
            <strong>⏱️ This code will expire in 10 minutes.</strong>
        </p>

        <p style='font-size: 14px; color: #777;'>
            If you didn't request this code, please ignore this email. Your account is secure.
        </p>

        <hr style='border: none; border-top: 1px solid #e0e0e0; margin: 30px 0;'>

        <p style='font-size: 12px; color: #999; text-align: center; margin: 0;'>
            © 2025 WeChat. All rights reserved.
        </p>
    </div>
</body>
</html>";
    }

    private string BuildVerificationEmailText(string firstName, string code)
    {
        return $@"Hi {firstName},

Thank you for registering with WeChat!

Your verification code is: {code}

This code will expire in 10 minutes.

If you didn't request this code, please ignore this email.

Best regards,
WeChat Team

© 2025 WeChat. All rights reserved.";
    }

    private string BuildPasswordResetEmailHtml(string firstName, string resetLink)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
        <h1 style='color: white; margin: 0; font-size: 28px;'>WeChat</h1>
    </div>

    <div style='background: #ffffff; padding: 40px; border: 1px solid #e0e0e0; border-top: none; border-radius: 0 0 10px 10px;'>
        <h2 style='color: #333; margin-top: 0;'>Hi {firstName},</h2>

        <p style='font-size: 16px; color: #555;'>
            We received a request to reset your WeChat password. Click the button below to reset it:
        </p>

        <div style='text-align: center; margin: 35px 0;'>
            <a href='{resetLink}' style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 15px 40px; text-decoration: none; border-radius: 5px; font-size: 16px; font-weight: bold; display: inline-block;'>
                Reset Password
            </a>
        </div>

        <p style='font-size: 14px; color: #777;'>
            Or copy and paste this link into your browser:
        </p>
        <p style='font-size: 12px; color: #667eea; word-break: break-all;'>
            {resetLink}
        </p>

        <p style='font-size: 14px; color: #777;'>
            <strong>⏱️ This link will expire in 1 hour.</strong>
        </p>

        <p style='font-size: 14px; color: #777;'>
            If you didn't request a password reset, please ignore this email or contact support if you have concerns.
        </p>

        <hr style='border: none; border-top: 1px solid #e0e0e0; margin: 30px 0;'>

        <p style='font-size: 12px; color: #999; text-align: center; margin: 0;'>
            © 2025 WeChat. All rights reserved.
        </p>
    </div>
</body>
</html>";
    }

    private string BuildPasswordResetEmailText(string firstName, string resetLink)
    {
        return $@"Hi {firstName},

We received a request to reset your WeChat password.

Click the link below to reset your password:
{resetLink}

This link will expire in 1 hour.

If you didn't request a password reset, please ignore this email.

Best regards,
WeChat Team

© 2025 WeChat. All rights reserved.";
    }

    private string BuildWelcomeEmailHtml(string firstName)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 40px; text-align: center; border-radius: 10px 10px 0 0;'>
        <h1 style='color: white; margin: 0; font-size: 32px;'>Welcome to WeChat! 🎉</h1>
    </div>

    <div style='background: #ffffff; padding: 40px; border: 1px solid #e0e0e0; border-top: none; border-radius: 0 0 10px 10px;'>
        <h2 style='color: #333; margin-top: 0;'>Hi {firstName},</h2>

        <p style='font-size: 16px; color: #555;'>
            Welcome to WeChat! We're thrilled to have you join our community. Get ready to connect, share, and explore!
        </p>

        <div style='background: #f8f9fa; padding: 25px; border-radius: 8px; margin: 30px 0;'>
            <h3 style='color: #667eea; margin-top: 0;'>Get Started:</h3>
            <ul style='color: #555; line-height: 2;'>
                <li><strong>Complete your profile</strong> - Add a photo and bio</li>
                <li><strong>Find friends</strong> - Connect with people you know</li>
                <li><strong>Share your first post</strong> - Let everyone know you're here!</li>
                <li><strong>Explore content</strong> - Discover what others are sharing</li>
            </ul>
        </div>

        <p style='font-size: 14px; color: #777;'>
            If you have any questions or need help, feel free to reach out to our support team.
        </p>

        <div style='text-align: center; margin: 35px 0;'>
            <a href='{_configuration["AppUrl"]}' style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 15px 40px; text-decoration: none; border-radius: 5px; font-size: 16px; font-weight: bold; display: inline-block;'>
                Go to WeChat
            </a>
        </div>

        <hr style='border: none; border-top: 1px solid #e0e0e0; margin: 30px 0;'>

        <p style='font-size: 12px; color: #999; text-align: center; margin: 0;'>
            © 2025 WeChat. All rights reserved.
        </p>
    </div>
</body>
</html>";
    }

    private string BuildWelcomeEmailText(string firstName)
    {
        return $@"Hi {firstName},

Welcome to WeChat! We're thrilled to have you join our community.

Get Started:
• Complete your profile - Add a photo and bio
• Find friends - Connect with people you know
• Share your first post - Let everyone know you're here!
• Explore content - Discover what others are sharing

If you have any questions or need help, feel free to reach out to our support team.

Best regards,
WeChat Team

© 2025 WeChat. All rights reserved.";
    }

    #endregion
}
