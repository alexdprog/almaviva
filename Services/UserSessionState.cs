namespace AlmavivaSlotChecker.Services;

public sealed class UserSessionState
{
    public bool IsAuthenticated { get; private set; }
    public string Email { get; private set; } = string.Empty;

    public void SignIn(string email)
    {
        IsAuthenticated = true;
        Email = email;
    }

    public void SignOut()
    {
        IsAuthenticated = false;
        Email = string.Empty;
    }
}
