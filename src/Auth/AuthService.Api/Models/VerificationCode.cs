namespace AuthService.Api.Models;

public class VerificationCode
{
    public Guid VerificationCodeId { get; set; }
    public Guid UserId { get; set; }
    public string Code { get; set; } = string.Empty;
    public VerificationType VerificationType { get; set; }
    public string Target { get; set; } = string.Empty;
    public bool IsUsed { get; set; }
    public bool IsExpired { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
}

public enum VerificationType
{
    Email,
    Phone
}
