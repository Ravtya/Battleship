using Microsoft.Extensions.Options;

namespace Battleship.Blazor.Services;

public sealed class ApiWakeService(HttpClient http, IOptions<BattleshipApiOptions> options)
{
    public async Task<bool> WakeAsync()
    {
        for (var i = 0; i < 20; i++)
        {
            try
            {
                using var response = await http.GetAsync(options.Value.HealthUrl);
                if (response.IsSuccessStatusCode)
                    return true;
            }
            catch (HttpRequestException) { }
            catch (TaskCanceledException) { }

            if (i < 19)
                await Task.Delay(3000);
        }

        return false;
    }
}
