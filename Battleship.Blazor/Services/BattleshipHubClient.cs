using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;

namespace Battleship.Blazor.Services;

public sealed class BattleshipHubClient(IOptions<BattleshipApiOptions> options) : IAsyncDisposable
{
    private readonly HubConnection _connection = new HubConnectionBuilder()
        .WithUrl(options.Value.HubUrl)
        .WithAutomaticReconnect()
        .Build();

    public async Task EnsureStartedAsync()
    {
        if (_connection.State is HubConnectionState.Connected or HubConnectionState.Connecting)
            return;
        await _connection.StartAsync();
    }

    public IDisposable On<T>(string method, Action<T> handler) => _connection.On(method, handler);

    public Task<T> InvokeAsync<T>(string method, params object?[] args) =>
        _connection.InvokeCoreAsync<T>(method, args, CancellationToken.None);

    public Task InvokeAsync(string method, params object?[] args) =>
        _connection.InvokeCoreAsync(method, args, CancellationToken.None);

    public async ValueTask DisposeAsync()
    {
        try
        {
            await _connection.StopAsync();
        }
        catch
        {
            // ignored
        }

        await _connection.DisposeAsync();
    }
}
