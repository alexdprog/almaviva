using AlmavivaSlotChecker.Models;

namespace AlmavivaSlotChecker.Services.Interfaces;

public interface ISlotCheckOrchestrator
{
    Task<SlotCheckResult> CheckNowAsync(CancellationToken cancellationToken = default);
    Task<SlotStatusViewModel> GetDashboardDataAsync(CancellationToken cancellationToken = default);
}
