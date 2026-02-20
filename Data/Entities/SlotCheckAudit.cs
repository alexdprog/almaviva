namespace AlmavivaSlotChecker.Data.Entities;

public sealed class SlotCheckAudit
{
    public long Id { get; set; }
    public string VisaCenterCode { get; set; } = string.Empty;
    public string ServiceCode { get; set; } = string.Empty;
    public bool HasSlots { get; set; }
    public string RawResponse { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
