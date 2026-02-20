namespace AlmavivaSlotChecker.Models;

public sealed class LoginRequest
{
    public string? Email { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
}
