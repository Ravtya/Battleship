using Battleship.Contracts;

namespace Battleship.Api.Models;

public class GameSession
{
    public required string Id { get; init; }
    public required string HostId { get; init; }
    public string? OpponentId { get; set; }
    public required string HostName { get; init; }

    public int GridSize { get; init; }
    public required List<ShipDefinition> ShipConfiguration { get; init; }
    public GamePhase Phase { get; set; } = GamePhase.WaitingForOpponent;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public PlayerBoard? HostBoard { get; set; }
    public PlayerBoard? OpponentBoard { get; set; }

    public string? CurrentTurnPlayerId { get; set; }
    public string? WinnerPlayerId { get; set; }

    public List<GameMove> MoveHistory { get; } = [];

    public string? GetOpponentId(string playerId) =>
        playerId == HostId ? OpponentId : playerId == OpponentId ? HostId : null;

    public PlayerBoard? GetBoard(string playerId) =>
        playerId == HostId ? HostBoard : playerId == OpponentId ? OpponentBoard : null;

    public PlayerBoard? GetOpponentBoard(string playerId) =>
        playerId == HostId ? OpponentBoard : playerId == OpponentId ? HostBoard : null;

    public LobbyGameDto ToLobbyDto() =>
        new(Id, HostId, HostName, GridSize, ShipConfiguration.Count);

    public GameStateDto ToDto(string playerId)
    {
        var own = GetBoard(playerId);
        var enemy = GetOpponentBoard(playerId);

        return new GameStateDto
        {
            GameId = Id,
            Phase = Phase,
            GridSize = GridSize,
            CurrentTurnPlayerId = CurrentTurnPlayerId,
            WinnerPlayerId = WinnerPlayerId,
            OwnBoard = own?.ToGrid(hideExcluded: true) ?? [],
            EnemyBoard = enemy?.ToGrid(hideShips: true) ?? [],
            ShipConfiguration = ShipConfiguration,
            PlacedShipIds = own?.Ships.Select(s => s.DefinitionId).ToList() ?? [],
            OwnPlacementComplete = own?.PlacementComplete ?? false,
            RecentMoves = MoveHistory.TakeLast(GridLimits.RecentMoveCount).ToList()
        };
    }
}