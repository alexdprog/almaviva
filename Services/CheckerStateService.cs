using AlmavivaSlotChecker.Models;

namespace AlmavivaSlotChecker.Services;

public sealed class CheckerStateService
{
    private readonly Lock _lock = new();
    private readonly List<string> _logs = [];
    private readonly List<string> _foundDates = [];

    public event Action? Changed;

    public AppStatus Status { get; private set; } = AppStatus.Idle;
    public DateTimeOffset? LastCheckAt { get; private set; }
    public bool AutoCheckingEnabled { get; private set; }

    public IReadOnlyList<string> Logs
    {
        get
        {
            lock (_lock)
            {
                return _logs.ToList();
            }
        }
    }

    public IReadOnlyList<string> FoundDates
    {
        get
        {
            lock (_lock)
            {
                return _foundDates.ToList();
            }
        }
    }

    public void AddLog(string message)
    {
        lock (_lock)
        {
            _logs.Add($"{DateTimeOffset.Now:HH:mm:ss} | {message}");
            if (_logs.Count > 200)
            {
                _logs.RemoveAt(0);
            }
        }
        Changed?.Invoke();
    }

    public void SetResult(SlotCheckResult result)
    {
        lock (_lock)
        {
            LastCheckAt = result.CheckedAt;
            if (result.HasSlots)
            {
                _foundDates.Clear();
                _foundDates.AddRange(result.Dates);
            }
        }

        Changed?.Invoke();
    }

    public void SetStatus(AppStatus status)
    {
        Status = status;
        Changed?.Invoke();
    }

    public void SetAutoChecking(bool enabled)
    {
        AutoCheckingEnabled = enabled;
        Changed?.Invoke();
    }
}
