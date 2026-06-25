namespace Battleship.Blazor.Services;

public sealed class BattleshipApiOptions
{
    public string HubUrl { get; set; } = "http://localhost:5169/Battle";

    public string HealthUrl
    {
        get
        {
                var baseUrl = HubUrl.TrimEnd('/');
                if (baseUrl.EndsWith("/Battle", StringComparison.OrdinalIgnoreCase))
                    baseUrl = baseUrl[..^"/Battle".Length];
                return $"{baseUrl}/health";
        }
    }
}