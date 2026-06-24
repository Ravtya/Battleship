namespace Battleship.Contracts;

public class FleetComposition
{
    public record ShipType(string Key, string Name, int Length);

    public static IReadOnlyList<ShipType> Types { get; } =
    [
        new("carrier", "Carrier", 4),
        new("battleship", "Battleship", 3),
        new("submarine", "Submarine", 2),
        new("destroyer", "Destroyer", 1),
    ];

    public static FleetComposition Classic { get; } = new(new Dictionary<string, int>
    {
        ["carrier"] = 1,
        ["battleship"] = 2,
        ["submarine"] = 3,
        ["destroyer"] = 4,
    });

    public Dictionary<string, int> CountsByType { get; set; } = new();

    public FleetComposition()
    {
    }

    public FleetComposition(Dictionary<string, int> countsByType) =>
        CountsByType = new Dictionary<string, int>(countsByType);

    public int TotalShips => CountsByType.Values.Sum();

    public int GetCount(string typeKey) =>
        CountsByType.GetValueOrDefault(typeKey, 0);

    public FleetComposition Clone() => new(CountsByType);

    public List<ShipDefinition> Build()
    {
        var result = new List<ShipDefinition>();

        foreach (var type in Types)
        {
            var count = GetCount(type.Key);
            for (var i = 1; i <= count; i++)
            {
                result.Add(new ShipDefinition($"{type.Key}-{i}", type.Name, type.Length));
            }
        }

        return result;
    }

    public (bool CanCreate, string Message) ValidateForGrid(int gridSize)
    {
        if (TotalShips == 0)
            return (false, "Add at least one ship.");

        return ShipPlacement.TryPlaceAll(gridSize, Build()) is not null
            ? (true, $"Fleet fits on a {gridSize}×{gridSize} grid.")
            : (false, "This fleet cannot fit on the selected grid.");
    }
}
