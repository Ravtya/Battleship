namespace Battleship.Blazor.Services;

public sealed class UserSession
{
    public string? PlayerId { get; set; }
    public string? DisplayName { get; set; }
    public bool IsRegistered => !string.IsNullOrWhiteSpace(PlayerId);
}
