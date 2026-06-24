namespace Battleship.Contracts;

public record PlayerInfo(string PlayerId, string Name);

public record ShipDefinition(string Id, string Name, int Length);

public record LobbyGameDto(string GameId, string HostId, string HostName, int GridSize, int ShipCount);

public record GameMove(string PlayerName, int Row, int Col, ShotResult Result);

public class GameStateDto
{
    public required string GameId { get; init; }
    public GamePhase Phase { get; init; }
    public int GridSize { get; init; }
    public string? CurrentTurnPlayerId { get; init; }
    public string? WinnerPlayerId { get; init; }
    public CellState[][] OwnBoard { get; init; } = [];
    public CellState[][] EnemyBoard { get; init; } = [];
    public List<ShipDefinition> ShipConfiguration { get; init; } = [];
    public List<string> PlacedShipIds { get; init; } = [];
    public bool OwnPlacementComplete { get; init; }
    public List<GameMove> RecentMoves { get; init; } = [];
}
