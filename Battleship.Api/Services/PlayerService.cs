using System.Collections.Concurrent;
using Battleship.Contracts;

namespace Battleship.Api.Services;

public class PlayerService
{
    private readonly ConcurrentDictionary<string, PlayerInfo> _players = new();
    private readonly Lock _nameLock = new();

    public PlayerInfo Register(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required");

        var uniqueName = GetUniqueName(name.Trim());
        var player = new PlayerInfo(Guid.NewGuid().ToString(), uniqueName);
        _players[player.PlayerId] = player;
        return player;
    }

    public void Unregister(string playerId) => _players.TryRemove(playerId, out _);

    public PlayerInfo? Get(string playerId) => _players.GetValueOrDefault(playerId);

    private string GetUniqueName(string name)
    {
        lock (_nameLock)
        {
            var used = _players.Values.Select(p => p.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (!used.Contains(name))
                return name;

            for (var i = 2; i < 10_000; i++)
            {
                var candidate = $"{name} {i}";
                if (!used.Contains(candidate))
                    return candidate;
            }

            return $"{name} {Guid.NewGuid().ToString("N")[..4]}";
        }
    }
}
