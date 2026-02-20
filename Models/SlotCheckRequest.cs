namespace AlmavivaSlotChecker.Models;

public sealed class SlotCheckRequest
{
    [Required]
    public string VisaCenterCode { get; set; } = "MOW";

    [Required]
    public string ServiceCode { get; set; } = "Schengen";
}
