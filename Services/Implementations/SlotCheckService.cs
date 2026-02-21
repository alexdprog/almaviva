using AlmavivaSlotChecker.Entities;
using AlmavivaSlotChecker.Models;
using AlmavivaSlotChecker.Services.Interfaces;

namespace AlmavivaSlotChecker.Services.Implementations;

public class SlotCheckService(IHttpClientFactory httpClientFactory, ILogger<SlotCheckService> logger) : ISlotCheckService
{
    public async Task<SlotCheckResult> CheckAsync(SlotCheckSettings settings, CancellationToken cancellationToken = default)
    {
        var result = new SlotCheckResult { CheckedAt = DateTimeOffset.UtcNow };

        try
        {
            var client = httpClientFactory.CreateClient();
            using var response = await client.GetAsync(settings.Url, cancellationToken);
            result.RawResponse = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                result.ErrorMessage = $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}";
                logger.LogError("HTTP error while checking slots: {StatusCode} {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
                return result;
            }

            result.IsAvailable = !string.Equals(result.RawResponse.Trim(), "false", StringComparison.OrdinalIgnoreCase);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected HTTP error while checking slots");
            result.ErrorMessage = ex.Message;
            return result;
        }
    }
}
