namespace Battleship.Blazor.Services;

public static class HubPage
{
    public static async Task RunAsync(BattleshipHubClient hub, Func<Task> action, Action<string?> setError)
    {
        setError(null);
        try
        {
            await hub.EnsureStartedAsync();
            await action();
        }
        catch (Exception ex)
        {
            setError(ex.Message);
        }
    }
}
