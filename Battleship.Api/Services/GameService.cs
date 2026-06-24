using System.Collections.Concurrent;
using Battleship.Api.Models;
using Battleship.Contracts;

namespace Battleship.Api.Services;

public class GameService
{
    private readonly ConcurrentDictionary<string, GameSession> _games = new();

    public IReadOnlyList<LobbyGameDto> BuildLobbyList() =>
        _games.Values
            .Where(g => g.Phase == GamePhase.WaitingForOpponent)
            .OrderByDescending(g => g.CreatedAt)
            .Select(g => g.ToLobbyDto())
            .ToList();

    public GameSession CreateGame(PlayerInfo host, int gridSize, FleetComposition fleet)
    {
        var game = new GameSession
        {
            Id = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant(),
            HostId = host.PlayerId,
            HostName = host.Name,
            GridSize = gridSize,
            ShipConfiguration = fleet.Build(),
            HostBoard = new PlayerBoard(gridSize),
        };

        _games[game.Id] = game;
        return game;
    }

    public GameSession JoinGame(string gameId, PlayerInfo opponent)
    {
        var game = Get(gameId);
        game.OpponentId = opponent.PlayerId;
        game.OpponentBoard = new PlayerBoard(game.GridSize);
        game.Phase = GamePhase.PlacingShips;
        return game;
    }

    public GameSession AutoPlace(string gameId, string playerId)
    {
        var (game, board) = GetGameWithBoard(gameId, playerId);

        if (!board.TryAutoPlace(game.ShipConfiguration))
            throw new InvalidOperationException("Could not auto-place ships on this grid.");

        FinishPlacement(game, board);
        return game;
    }

    public GameSession PlaceShip(string gameId, string playerId, string shipId, int row, int col, bool horizontal)
    {
        var (game, board) = GetGameWithBoard(gameId, playerId);
        var ship = game.ShipConfiguration.First(s => s.Id == shipId);
        board.PlaceShip(ship, row, col, horizontal);
        return game;
    }

    public GameSession ConfirmPlacement(string gameId, string playerId)
    {
        var (game, board) = GetGameWithBoard(gameId, playerId);
        FinishPlacement(game, board);
        return game;
    }

    public GameSession Fire(string gameId, PlayerInfo player, int row, int col)
    {
        var (game, target) = GetGameWithOpponentBoard(gameId, player.PlayerId);
        var result = target.Fire(row, col);

        game.MoveHistory.Add(new GameMove(player.Name, row, col, result));

        if (target.AllShipsSunk)
        {
            game.WinnerPlayerId = player.PlayerId;
            game.Phase = GamePhase.Finished;
        }
        else if (result is ShotResult.Miss)
        {
            game.CurrentTurnPlayerId = game.GetOpponentId(player.PlayerId);
        }

        return game;
    }

    public GameSession Get(string gameId) =>
        _games.TryGetValue(gameId, out var game)
            ? game
            : throw new InvalidOperationException("Game not found.");

    private (GameSession Game, PlayerBoard Board) GetGameWithBoard(string gameId, string playerId)
    {
        var game = Get(gameId);
        var board = game.GetBoard(playerId) ?? throw new InvalidOperationException("Player is not part of this game.");
        return (game, board);
    }

    private (GameSession Game, PlayerBoard Board) GetGameWithOpponentBoard(string gameId, string playerId)
    {
        var game = Get(gameId);
        var board = game.GetOpponentBoard(playerId) ?? throw new InvalidOperationException("Opponent not found.");
        return (game, board);
    }

    private static void FinishPlacement(GameSession game, PlayerBoard board)
    {
        board.PlacementComplete = true;

        if (game.HostBoard?.PlacementComplete != true || game.OpponentBoard?.PlacementComplete != true)
            return;

        game.Phase = GamePhase.InProgress;
        game.CurrentTurnPlayerId = game.HostId;
    }
}
