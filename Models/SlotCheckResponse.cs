namespace AlmavivaSlotChecker.Models;

public sealed class SlotCheckResponse
{
    public bool HasSlots { get; set; }
    public string RawResponse { get; set; } = string.Empty;
    public DateTime CheckedAtUtc { get; set; } = DateTime.UtcNow;
}
