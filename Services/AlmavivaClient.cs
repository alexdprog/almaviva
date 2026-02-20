using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AlmavivaSlotChecker.Models;

namespace AlmavivaSlotChecker.Services;

public sealed class AlmavivaClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<bool> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        // Реальные поля можно уточнить по network-инспектору сайта.
        var payload = new
        {
            email = request.Email,
            password = request.Password
        };

        using var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        using var response = await httpClient.PostAsync("api/account/login", content, cancellationToken);

        return response.IsSuccessStatusCode;
    }

    public async Task<SlotCheckResponse> CheckSlotsAsync(SlotCheckRequest request, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            visaCenterCode = request.VisaCenterCode,
            serviceCode = request.ServiceCode
        };

        using var response = await httpClient.PostAsJsonAsync("api/appointment/check", payload, JsonOptions, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        var hasSlots = false;

        if (response.IsSuccessStatusCode)
        {
            if (bool.TryParse(body.Trim(), out var result))
            {
                hasSlots = result;
            }
            else
            {
                try
                {
                    using var json = JsonDocument.Parse(body);
                    if (json.RootElement.TryGetProperty("available", out var available))
                    {
                        hasSlots = available.GetBoolean();
                    }
                }
                catch (JsonException)
                {
                    // ignore parse errors, сохраняем raw body
                }
            }
        }

        return new SlotCheckResponse
        {
            HasSlots = hasSlots,
            RawResponse = body,
            CheckedAtUtc = DateTime.UtcNow
        };
    }
}
