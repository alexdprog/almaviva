namespace AlmavivaSlotChecker.Data.Entities;

public sealed class UserLoginAudit
{
    public long Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
