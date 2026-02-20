using System.Collections.Concurrent;

namespace AlmavivaSlotChecker.Services;

public sealed class OAuthStateStore
{
    private readonly ConcurrentDictionary<string, DateTime> _states = new();

    public string Create()
    {
        var state = Convert.ToHexString(Guid.NewGuid().ToByteArray());
        _states[state] = DateTime.UtcNow;
        return state;
    }

    public bool ValidateAndConsume(string? state)
    {
        if (string.IsNullOrWhiteSpace(state))
        {
            return false;
        }

        if (!_states.TryRemove(state, out var createdAtUtc))
        {
            return false;
        }

        return DateTime.UtcNow - createdAtUtc < TimeSpan.FromMinutes(10);
    }
}
