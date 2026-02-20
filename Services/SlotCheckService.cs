using System.Text.Json;
using AlmavivaSlotChecker.Models;

namespace AlmavivaSlotChecker.Services;

public sealed class SlotCheckService(
    IHttpClientFactory httpClientFactory,
    AuthService authService,
    CheckerStateService state,
    TelegramNotificationService telegram,
    ILogger<SlotCheckService> logger)
{
    private readonly SemaphoreSlim _checkLock = new(1, 1);

    public async Task<SlotCheckResult> CheckNowAsync(CancellationToken cancellationToken = default)
    {
        await _checkLock.WaitAsync(cancellationToken);
        state.SetStatus(AppStatus.Checking);

        try
        {
            var token = await authService.GetValidAccessTokenAsync(cancellationToken);
            using var request = new HttpRequestMessage(HttpMethod.Get,
                "reservation-manager/api/planning/v1/checks-multiple?officeId=3&visaIdList=1,1,1&serviceLevelId=1");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            using var response = await httpClientFactory.CreateClient("visa-api").SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            response.EnsureSuccessStatusCode();

            var hasSlots = !string.Equals(body.Trim(), "false", StringComparison.OrdinalIgnoreCase);
            var dates = hasSlots ? ExtractDateStrings(body) : [];

            var result = new SlotCheckResult
            {
                HasSlots = hasSlots,
                Dates = dates,
                RawResponse = body,
                CheckedAt = DateTimeOffset.UtcNow
            };

            state.SetResult(result);
            state.SetStatus(AppStatus.Idle);
            state.AddLog($"Check completed. Slots found: {hasSlots}.");

            if (hasSlots)
            {
                await telegram.SendMessageAsync($"Найдены слоты: {string.Join(", ", dates.DefaultIfEmpty("см. raw response"))}", cancellationToken);
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Slot check failed");
            var result = new SlotCheckResult
            {
                Error = ex.Message,
                CheckedAt = DateTimeOffset.UtcNow
            };
            state.SetResult(result);
            state.SetStatus(AppStatus.Error);
            state.AddLog($"Check failed: {ex.Message}");
            return result;
        }
        finally
        {
            _checkLock.Release();
        }
    }

    private static List<string> ExtractDateStrings(string json)
    {
        var result = new List<string>();

        try
        {
            using var doc = JsonDocument.Parse(json);
            Walk(doc.RootElement, result);
        }
        catch (JsonException)
        {
            // Raw body can be non-json in edge cases; still considered as slot found.
        }

        return result.Distinct().ToList();
    }

    private static void Walk(JsonElement element, ICollection<string> results)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Array:
                foreach (var child in element.EnumerateArray()) Walk(child, results);
                break;
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject()) Walk(property.Value, results);
                break;
            case JsonValueKind.String:
                var value = element.GetString();
                if (DateTimeOffset.TryParse(value, out _))
                {
                    results.Add(value!);
                }
                break;
        }
    }
}
