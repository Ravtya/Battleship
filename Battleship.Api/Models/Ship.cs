namespace Battleship.Api.Models;

public class Ship
{
    public required string DefinitionId { get; init; }
    public required int Length { get; init; }
    public required int Row { get; init; }
    public required int Col { get; init; }
    public required bool Horizontal { get; init; }
    public bool IsSunk { get; set; }
}
