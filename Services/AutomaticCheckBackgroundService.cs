namespace AlmavivaSlotChecker.Services;

public sealed class AutomaticCheckBackgroundService(
    AutoCheckCoordinator coordinator,
    CheckerStateService state,
    ILogger<AutomaticCheckBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(60));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
                await coordinator.ExecuteIfEnabledAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Automatic checking failed");
                state.SetStatus(Models.AppStatus.Error);
                state.AddLog($"Automatic checking failed: {ex.Message}");
            }
        }
    }
}
