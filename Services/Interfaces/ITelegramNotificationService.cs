namespace AlmavivaSlotChecker.Services.Interfaces;

public interface ITelegramNotificationService
{
    Task SendMessageAsync(string message, string botToken, string chatId, CancellationToken cancellationToken = default);
}
