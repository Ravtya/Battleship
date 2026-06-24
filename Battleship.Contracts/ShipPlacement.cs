namespace Battleship.Contracts;

public readonly record struct ResolvedPlacement(string ShipId, int Row, int Col, bool Horizontal);

public static class ShipPlacement
{
    public static bool IsComplete(IReadOnlyList<ShipDefinition> configuration, IEnumerable<string> placedIds) =>
        configuration.All(s => placedIds.Contains(s.Id));

    public static IEnumerable<(int Row, int Col)> Cells(int length, int row, int col, bool horizontal)
    {
        for (var i = 0; i < length; i++)
            yield return horizontal ? (row, col + i) : (row + i, col);
    }

    public static bool Validate(
        int gridSize,
        int shipLength,
        int row,
        int col,
        bool horizontal,
        Func<int, int, CellState> readCell)
    {
        if (shipLength <= 0)
            return false;

        var candidateCells = new HashSet<(int Row, int Col)>();
        foreach (var (r, c) in Cells(shipLength, row, col, horizontal))
        {
            if (r < 0 || c < 0 || r >= gridSize || c >= gridSize)
                return false;

            candidateCells.Add((r, c));
        }

        foreach (var (r, c) in candidateCells)
        {
            if (readCell(r, c) != CellState.Empty)
                return false;
        }

        foreach (var (r, c) in candidateCells)
        {
            for (var dr = -1; dr <= 1; dr++)
            for (var dc = -1; dc <= 1; dc++)
            {
                if (dr == 0 && dc == 0)
                    continue;

                var nr = r + dr;
                var nc = c + dc;
                if (nr < 0 || nc < 0 || nr >= gridSize || nc >= gridSize)
                    continue;

                if (readCell(nr, nc) == CellState.Ship && !candidateCells.Contains((nr, nc)))
                    return false;
            }
        }

        return true;
    }

    public static (HashSet<(int Row, int Col)> Cells, bool IsValid) Preview(
        int gridSize,
        ShipDefinition ship,
        (int Row, int Col) start,
        bool horizontal,
        CellState[][] board)
    {
        var cells = Cells(ship.Length, start.Row, start.Col, horizontal)
            .Where(c => c.Row >= 0 && c.Row < gridSize && c.Col >= 0 && c.Col < gridSize)
            .ToHashSet();

        var isValid = Validate(
            gridSize,
            ship.Length,
            start.Row,
            start.Col,
            horizontal,
            (r, c) => board[r][c]);

        return (cells, isValid);
    }

    public static IReadOnlyList<ResolvedPlacement>? TryPlaceAll(
        int gridSize,
        IReadOnlyList<ShipDefinition> ships)
    {
        if (ships.Count == 0
            || ships.Any(s => s.Length > gridSize)
            || ships.Sum(s => s.Length) > gridSize * gridSize)
            return null;

        var cells = new CellState[gridSize, gridSize];
        var placements = new List<ResolvedPlacement>();

        foreach (var ship in ships.OrderByDescending(s => s.Length))
        {
            if (!TryPlaceShip(gridSize, ship, cells, placements))
                return null;
        }

        return placements;
    }

    private static bool TryPlaceShip(
        int gridSize,
        ShipDefinition ship,
        CellState[,] cells,
        List<ResolvedPlacement> placements)
    {
        for (var row = 0; row < gridSize; row++)
        for (var col = 0; col < gridSize; col++)
            foreach (var horizontal in new[] { false, true })
            {
                if (TryPaintShip(gridSize, ship, row, col, horizontal, cells, placements))
                    return true;
            }

        return false;
    }

    private static bool TryPaintShip(
        int gridSize,
        ShipDefinition ship,
        int row,
        int col,
        bool horizontal,
        CellState[,] cells,
        List<ResolvedPlacement> placements)
    {
        if (!Validate(gridSize, ship.Length, row, col, horizontal, (r, c) => cells[r, c]))
            return false;

        foreach (var (r, c) in Cells(ship.Length, row, col, horizontal))
            cells[r, c] = CellState.Ship;

        placements.Add(new ResolvedPlacement(ship.Id, row, col, horizontal));
        return true;
    }
}
