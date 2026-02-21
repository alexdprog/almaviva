namespace AlmavivaSlotChecker.Models;

public class SlotStatusViewModel
{
    public string StatusText { get; set; } = "Not Checked";
    public DateTimeOffset? LastCheckedAt { get; set; }
    public List<SlotCheckResult> Logs { get; set; } = [];
}
