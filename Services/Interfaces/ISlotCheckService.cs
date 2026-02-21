using AlmavivaSlotChecker.Entities;
using AlmavivaSlotChecker.Models;

namespace AlmavivaSlotChecker.Services.Interfaces;

public interface ISlotCheckService
{
    Task<SlotCheckResult> CheckAsync(SlotCheckSettings settings, CancellationToken cancellationToken = default);
}
