namespace AlmavivaSlotChecker.Entities;

public class SlotCheckSettings
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public int CheckIntervalMinutes { get; set; } = 5;
    public string TelegramBotToken { get; set; } = string.Empty;
    public string TelegramChatId { get; set; } = string.Empty;
}
