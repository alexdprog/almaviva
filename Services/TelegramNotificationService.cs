using AlmavivaSlotChecker.Models;
using Microsoft.Extensions.Options;

namespace AlmavivaSlotChecker.Services;

public sealed class TelegramNotificationService(
    IHttpClientFactory httpClientFactory,
    IOptions<TelegramOptions> options,
    ILogger<TelegramNotificationService> logger)
{
    private readonly TelegramOptions _options = options.Value;

    public async Task SendMessageAsync(string message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.BotToken) || string.IsNullOrWhiteSpace(_options.ChatId))
        {
            logger.LogInformation("Telegram is not configured; notification skipped.");
            return;
        }

        var response = await httpClientFactory.CreateClient("telegram")
            .PostAsync($"bot{_options.BotToken}/sendMessage?chat_id={_options.ChatId}&text={Uri.EscapeDataString(message)}", null, cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}
