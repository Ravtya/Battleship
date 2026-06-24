using Battleship.Contracts;

namespace Battleship.Api.Models;

public class PlayerBoard(int size)
{
    private readonly CellState[,] _cells = new CellState[size, size];
    public List<Ship> Ships { get; } = [];
    public bool PlacementComplete { get; set; }
    private int Size { get; } = size;

    public void PlaceShip(ShipDefinition ship, int row, int col, bool horizontal)
    {
        Paint(ship.Length, row, col, horizontal, CellState.Ship);
        Ships.Add(new Ship
        {
            DefinitionId = ship.Id,
            Length = ship.Length,
            Row = row,
            Col = col,
            Horizontal = horizontal
        });
    }

    public ShotResult Fire(int row, int col)
    {
        if (_cells[row, col] == CellState.Empty)
        {
            _cells[row, col] = CellState.Miss;
            return ShotResult.Miss;
        }

        _cells[row, col] = CellState.Hit;
        var ship = Ships.First(s => CoversCell(s, row, col));

        if (!IsSunk(ship))
            return ShotResult.Hit;

        ship.IsSunk = true;
        Paint(ship.Length, ship.Row, ship.Col, ship.Horizontal, CellState.Sunk);
        MarkExcludedAround(ship);
        return ShotResult.Sunk;
    }

    public bool AllShipsSunk => Ships.Count > 0 && Ships.All(s => s.IsSunk);

    public CellState[][] ToGrid(bool hideShips = false, bool hideExcluded = false)
    {
        var grid = new CellState[Size][];
        for (var r = 0; r < Size; r++)
        {
            grid[r] = new CellState[Size];
            for (var c = 0; c < Size; c++)
            {
                var cell = _cells[r, c];
                if (hideShips && cell == CellState.Ship)
                    cell = CellState.Empty;
                if (hideExcluded && cell == CellState.Excluded)
                    cell = CellState.Empty;
                grid[r][c] = cell;
            }
        }

        return grid;
    }

    public bool TryAutoPlace(IReadOnlyList<ShipDefinition> ships)
    {
        Clear();
        var placements = ShipPlacement.TryPlaceAll(Size, ships);
        if (placements is null)
            return false;

        var shipsById = ships.ToDictionary(s => s.Id);
        foreach (var placement in placements)
        {
            var ship = shipsById[placement.ShipId];
            PlaceShip(ship, placement.Row, placement.Col, placement.Horizontal);
        }

        return true;
    }

    private void Clear()
    {
        Ships.Clear();
        Array.Clear(_cells);
        PlacementComplete = false;
    }

    private void Paint(int length, int row, int col, bool horizontal, CellState state)
    {
        foreach (var (r, c) in ShipPlacement.Cells(length, row, col, horizontal))
            _cells[r, c] = state;
    }

    private static bool CoversCell(Ship ship, int row, int col) =>
        ShipPlacement.Cells(ship.Length, ship.Row, ship.Col, ship.Horizontal)
            .Any(cell => cell.Row == row && cell.Col == col);

    private bool IsSunk(Ship ship) =>
        ShipPlacement.Cells(ship.Length, ship.Row, ship.Col, ship.Horizontal)
            .All(cell => _cells[cell.Row, cell.Col] == CellState.Hit);

    private void MarkExcludedAround(Ship ship)
    {
        foreach (var (r, c) in ShipPlacement.Cells(ship.Length, ship.Row, ship.Col, ship.Horizontal))
        {
            for (var dr = -1; dr <= 1; dr++)
            for (var dc = -1; dc <= 1; dc++)
            {
                var nr = r + dr;
                var nc = c + dc;
                if (nr >= 0 && nr < Size && nc >= 0 && nc < Size && _cells[nr, nc] == CellState.Empty)
                    _cells[nr, nc] = CellState.Excluded;
            }
        }
    }
}
