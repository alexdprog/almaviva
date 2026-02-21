namespace AlmavivaSlotChecker.Entities;

public class SlotCheckLog
{
    public long Id { get; set; }
    public DateTimeOffset CheckedAt { get; set; }
    public bool IsAvailable { get; set; }
    public string RawResponse { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}
