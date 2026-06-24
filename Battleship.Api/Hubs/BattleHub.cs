using Microsoft.AspNetCore.SignalR;
using Battleship.Api.Models;
using Battleship.Api.Services;
using Battleship.Contracts;

namespace Battleship.Api.Hubs;

public class BattleHub(GameService gameService, PlayerService playerService) : Hub
{
    private const string LobbyGroup = "lobby";
    private const string PlayerIdKey = "playerId";

    private static string PlayerGroup(string playerId) => $"player:{playerId}";

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (TryGetPlayerId() is { } playerId)
            playerService.Unregister(playerId);

        Context.Items.Remove(PlayerIdKey);
        return base.OnDisconnectedAsync(exception);
    }

    public async Task<PlayerInfo> Register(string name)
    {
        var player = playerService.Register(name);
        Context.Items[PlayerIdKey] = player.PlayerId;
        await Groups.AddToGroupAsync(Context.ConnectionId, LobbyGroup);
        await Groups.AddToGroupAsync(Context.ConnectionId, PlayerGroup(player.PlayerId));

        return player;
    }

    public IReadOnlyList<LobbyGameDto> GetLobby() => gameService.BuildLobbyList();

    public async Task<string> CreateGame(int gridSize, FleetComposition fleet)
    {
        var game = await ExecuteAndNotify(player => gameService.CreateGame(player, gridSize, fleet), notifyLobby: true);
        return game.Id;
    }

    public Task JoinGame(string gameId) =>
        ExecuteAndNotify(player => gameService.JoinGame(gameId, player), notifyLobby: true);

    public Task AutoPlace(string gameId) =>
        ExecuteAndNotify(player => gameService.AutoPlace(gameId, player.PlayerId));

    public Task PlaceShip(string gameId, string shipId, int row, int col, bool horizontal) =>
        ExecuteAndNotify(player => gameService.PlaceShip(gameId, player.PlayerId, shipId, row, col, horizontal));

    public Task ConfirmPlacement(string gameId) =>
        ExecuteAndNotify(player => gameService.ConfirmPlacement(gameId, player.PlayerId));

    public Task Fire(string gameId, int row, int col) =>
        ExecuteAndNotify(player => gameService.Fire(gameId, player, row, col));

    private async Task<GameSession> ExecuteAndNotify(Func<PlayerInfo, GameSession> action, bool notifyLobby = false)
    {
        GameSession game;
        try
        {
            var player = GetPlayer();
            game = action(player);
        }
        catch (Exception ex)
        {
            throw new HubException(ex.Message);
        }

        if (notifyLobby)
            await NotifyLobbyUpdated();

        await NotifyGamePlayers(game);
        return game;
    }

    private PlayerInfo GetPlayer() =>
        TryGetPlayerId() is { } playerId && playerService.Get(playerId) is { } player
            ? player
            : throw new HubException("Not registered.");

    private string? TryGetPlayerId() =>
        Context.Items.TryGetValue(PlayerIdKey, out var value) && value is string playerId
            ? playerId
            : null;

    private Task NotifyLobbyUpdated() =>
        Clients.Group(LobbyGroup).SendAsync("LobbyUpdated", gameService.BuildLobbyList());

    private Task NotifyGamePlayers(GameSession game) =>
        Task.WhenAll(
            SendGameState(game, game.HostId),
            game.OpponentId is not null ? SendGameState(game, game.OpponentId) : Task.CompletedTask);

    private Task SendGameState(GameSession game, string playerId) =>
        Clients.Group(PlayerGroup(playerId)).SendAsync("GameUpdated", game.ToDto(playerId));

    public GameStateDto GetGameState(string gameId) => gameService.Get(gameId).ToDto(GetPlayer().PlayerId);
}