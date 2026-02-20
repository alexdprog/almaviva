namespace AlmavivaSlotChecker.Services;

public sealed class AutoCheckCoordinator
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private Func<CancellationToken, Task>? _checkAction;

    public bool Enabled { get; private set; }

    public void Start(Func<CancellationToken, Task> checkAction)
    {
        _checkAction = checkAction;
        Enabled = true;
    }

    public void Stop() => Enabled = false;

    public async Task ExecuteIfEnabledAsync(CancellationToken cancellationToken)
    {
        if (!Enabled || _checkAction is null)
        {
            return;
        }

        if (!await _gate.WaitAsync(0, cancellationToken))
        {
            return;
        }

        try
        {
            await _checkAction(cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }
}
