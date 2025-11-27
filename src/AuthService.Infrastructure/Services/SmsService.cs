using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using AuthService.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AuthService.Infrastructure.Services;

/// <summary>
/// SMS service implementation using Twilio
/// Falls back to console logging if Twilio is not configured
/// </summary>
public class SmsService : ISmsService
{
    private readonly ILogger<SmsService> _logger;
    private readonly IConfiguration _configuration;
    private readonly bool _isConfigured;
    private readonly string? _accountSid;
    private readonly string? _authToken;
    private readonly string? _fromPhoneNumber;

    public SmsService(ILogger<SmsService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;

        // Check if Twilio is configured
        _accountSid = _configuration["Twilio:AccountSid"];
        _authToken = _configuration["Twilio:AuthToken"];
        _fromPhoneNumber = _configuration["Twilio:PhoneNumber"];

        _isConfigured = !string.IsNullOrEmpty(_accountSid) &&
                       !string.IsNullOrEmpty(_authToken) &&
                       !string.IsNullOrEmpty(_fromPhoneNumber) &&
                       _accountSid != "your-twilio-account-sid-here";

        if (_isConfigured)
        {
            // Initialize Twilio client
            TwilioClient.Init(_accountSid, _authToken);
            _logger.LogInformation("Twilio SMS service initialized successfully");
        }
        else
        {
            _logger.LogWarning(
                "Twilio is not configured. SMS sending will fall back to console logging. " +
                "Please configure Twilio settings in appsettings.json for production use.");
        }
    }

    public async Task<bool> SendVerificationCodeAsync(string phoneNumber, string code)
    {
        try
        {
            var message = $"Your WeChat verification code is: {code}. This code will expire in 10 minutes.";

            if (_isConfigured)
            {
                return await SendViaTwilio(phoneNumber, message);
            }
            else
            {
                return LogSmsToConsole(phoneNumber, "Verification Code", code);
            }
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
            var message = $"Your WeChat password reset code is: {code}. This code will expire in 10 minutes.";

            if (_isConfigured)
            {
                return await SendViaTwilio(phoneNumber, message);
            }
            else
            {
                _logger.LogInformation(
                    "=== SMS PASSWORD RESET CODE (CONSOLE MODE) ===\n" +
                    "To: {PhoneNumber}\n" +
                    "---\n" +
                    "Your WeChat password reset code is: {Code}\n\n" +
                    "This code will expire in 10 minutes.\n" +
                    "===============================================",
                    phoneNumber, code);
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending password reset SMS to {PhoneNumber}", phoneNumber);
            return false;
        }
    }

    #region Private Helper Methods

    private async Task<bool> SendViaTwilio(string phoneNumber, string messageBody)
    {
        try
        {
            var message = await MessageResource.CreateAsync(
                body: messageBody,
                from: new PhoneNumber(_fromPhoneNumber),
                to: new PhoneNumber(phoneNumber)
            );

            // Check message status
            if (message.Status == MessageResource.StatusEnum.Failed ||
                message.Status == MessageResource.StatusEnum.Undelivered)
            {
                _logger.LogError(
                    "Twilio failed to send SMS to {PhoneNumber}. Status: {Status}, ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}",
                    phoneNumber, message.Status, message.ErrorCode, message.ErrorMessage);
                return false;
            }

            _logger.LogInformation(
                "SMS sent successfully via Twilio to {PhoneNumber}. MessageSid: {MessageSid}, Status: {Status}",
                phoneNumber, message.Sid, message.Status);
            return true;
        }
        catch (Twilio.Exceptions.ApiException ex)
        {
            _logger.LogError(
                ex,
                "Twilio API exception sending SMS to {PhoneNumber}. Code: {Code}, Message: {Message}",
                phoneNumber, ex.Code, ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception sending SMS via Twilio to {PhoneNumber}", phoneNumber);
            return false;
        }
    }

    private bool LogSmsToConsole(string phoneNumber, string purpose, string code)
    {
        _logger.LogInformation(
            "=== SMS (CONSOLE MODE) ===\n" +
            "To: {PhoneNumber}\n" +
            "Purpose: {Purpose}\n" +
            "---\n" +
            "Your WeChat verification code is: {Code}\n\n" +
            "This code will expire in 10 minutes.\n" +
            "==========================",
            phoneNumber, purpose, code);
        return true;
    }

    #endregion
}
