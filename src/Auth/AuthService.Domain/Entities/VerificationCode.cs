using AuthService.Domain.Enums;

namespace AuthService.Domain.Entities;

public class VerificationCode
{
    public Guid VerificationCodeId { get; set; }
    public Guid UserId { get; set; }
    public string Code { get; set; } = string.Empty;
    public VerificationType VerificationType { get; set; }
    public string Target { get; set; } = string.Empty; // Email or Phone number
    public bool IsUsed { get; set; }
    public bool IsExpired { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }

    // Helper methods
    public bool IsValid()
    {
        return !IsUsed && !IsExpired && ExpiresAt > DateTime.UtcNow;
    }

    public void MarkAsUsed()
    {
        IsUsed = true;
        UsedAt = DateTime.UtcNow;
    }

    public void MarkAsExpired()
    {
        IsExpired = true;
    }
}
