namespace AuthService.Domain.Entities;

public class PasswordResetToken
{
    public Guid TokenId { get; set; }
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UsedAt { get; set; }

    // Helper methods
    public bool IsValid()
    {
        return !IsUsed && ExpiresAt > DateTime.UtcNow;
    }

    public void MarkAsUsed()
    {
        IsUsed = true;
        UsedAt = DateTime.UtcNow;
    }
}
