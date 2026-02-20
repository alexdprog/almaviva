namespace AlmavivaSlotChecker.Models;

public sealed class SlotCheckResult
{
    public bool HasSlots { get; set; }
    public List<string> Dates { get; set; } = [];
    public string RawResponse { get; set; } = string.Empty;
    public DateTimeOffset CheckedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? Error { get; set; }
}
