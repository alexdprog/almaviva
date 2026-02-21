using AlmavivaSlotChecker.Data;
using AlmavivaSlotChecker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AlmavivaSlotChecker.Services.Hosted;

public class SlotCheckBackgroundService(IServiceScopeFactory scopeFactory, ILogger<SlotCheckBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var orchestrator = scope.ServiceProvider.GetRequiredService<ISlotCheckOrchestrator>();

                var settings = await dbContext.SlotCheckSettings.AsNoTracking().FirstOrDefaultAsync(stoppingToken);
                var interval = Math.Max(1, settings?.CheckIntervalMinutes ?? 5);

                await orchestrator.CheckNowAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromMinutes(interval), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Background slot check failed");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
