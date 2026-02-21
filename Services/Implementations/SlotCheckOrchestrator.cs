using AlmavivaSlotChecker.Data;
using AlmavivaSlotChecker.Entities;
using AlmavivaSlotChecker.Models;
using AlmavivaSlotChecker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AlmavivaSlotChecker.Services.Implementations;

public class SlotCheckOrchestrator(
    ApplicationDbContext dbContext,
    ISlotCheckService slotCheckService,
    ITelegramNotificationService telegramNotificationService,
    ILogger<SlotCheckOrchestrator> logger) : ISlotCheckOrchestrator
{
    public async Task<SlotCheckResult> CheckNowAsync(CancellationToken cancellationToken = default)
    {
        var settings = await GetOrCreateSettingsAsync(cancellationToken);
        var previous = await dbContext.SlotCheckLogs.OrderByDescending(x => x.CheckedAt).FirstOrDefaultAsync(cancellationToken);

        var result = await slotCheckService.CheckAsync(settings, cancellationToken);
        var log = new SlotCheckLog
        {
            CheckedAt = result.CheckedAt,
            IsAvailable = result.IsAvailable,
            RawResponse = result.RawResponse,
            ErrorMessage = result.ErrorMessage
        };

        dbContext.SlotCheckLogs.Add(log);
        await dbContext.SaveChangesAsync(cancellationToken);

        var changed = previous is null || previous.IsAvailable != result.IsAvailable;
        if (result.IsAvailable || changed)
        {
            var statusText = result.IsAvailable ? "Available" : "Not Available";
            await telegramNotificationService.SendMessageAsync(
                $"Slot status: {statusText}. Checked at {result.CheckedAt:u}",
                settings.TelegramBotToken,
                settings.TelegramChatId,
                cancellationToken);
        }

        logger.LogInformation("Slot check completed. Available: {IsAvailable}; Error: {Error}", result.IsAvailable, result.ErrorMessage);
        return result;
    }

    public async Task<SlotStatusViewModel> GetDashboardDataAsync(CancellationToken cancellationToken = default)
    {
        var logs = await dbContext.SlotCheckLogs
            .OrderByDescending(x => x.CheckedAt)
            .Take(50)
            .Select(x => new SlotCheckResult
            {
                CheckedAt = x.CheckedAt,
                IsAvailable = x.IsAvailable,
                RawResponse = x.RawResponse,
                ErrorMessage = x.ErrorMessage
            })
            .ToListAsync(cancellationToken);

        var latest = logs.FirstOrDefault();

        return new SlotStatusViewModel
        {
            StatusText = latest is null
                ? "Not Checked"
                : latest.ErrorMessage is not null
                    ? "Error"
                    : latest.IsAvailable ? "Available" : "Not Available",
            LastCheckedAt = latest?.CheckedAt,
            Logs = logs
        };
    }

    private async Task<SlotCheckSettings> GetOrCreateSettingsAsync(CancellationToken cancellationToken)
    {
        var settings = await dbContext.SlotCheckSettings.FirstOrDefaultAsync(cancellationToken);
        if (settings is not null)
        {
            return settings;
        }

        settings = new SlotCheckSettings
        {
            Url = "https://example.org/slots",
            CheckIntervalMinutes = 5
        };

        dbContext.SlotCheckSettings.Add(settings);
        await dbContext.SaveChangesAsync(cancellationToken);
        return settings;
    }
}
