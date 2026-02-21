using System.Net.Http.Json;
using AlmavivaSlotChecker.Services.Interfaces;

namespace AlmavivaSlotChecker.Services.Implementations;

public class TelegramNotificationService(IHttpClientFactory httpClientFactory, ILogger<TelegramNotificationService> logger) : ITelegramNotificationService
{
    public async Task SendMessageAsync(string message, string botToken, string chatId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(botToken) || string.IsNullOrWhiteSpace(chatId))
        {
            return;
        }

        try
        {
            var client = httpClientFactory.CreateClient();
            var payload = new { chat_id = chatId, text = message };
            await client.PostAsJsonAsync($"https://api.telegram.org/bot{botToken}/sendMessage", payload, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Telegram sendMessage failed");
        }
    }
}
