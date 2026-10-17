using trevor.Core;
using trevor.Infrastructure.Discord;
using trevor.Infrastructure.Discord.Models;

namespace trevor.Features.Teams;

public class TeamsCommand : ICommand
{
    public string Name => "teams";
    public bool IsDeferred => false;

    public Task<string> ExecuteAsync(DiscordInteraction interaction, CancellationToken ct = default)
    {
        var players = ParsePlayers(interaction.GetOption("gracze"));
        if (players.Length == 0)
            return Task.FromResult("Podaj graczy po przecinku, np. `Gracz1,Gracz2,Gracz3,Gracz4`.");

        var (blue, red) = Split(players, Random.Shared);

        return Task.FromResult($"""
                                🏆 **Wynik losowania drużyn**

                                🔵 **Niebiescy**
                                —————————————---
                                {string.Join("\n", blue.Select(p => $"• {p}"))}

                                🔴 **Czerwoni**
                                —————————————---
                                {string.Join("\n", red.Select(p => $"• {p}"))}
                                """);
    }

    public static string[] ParsePlayers(string? raw) =>
        (raw ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToArray();

    // Shuffles, then deals players alternately so team sizes differ by at most one
    public static (List<string> Blue, List<string> Red) Split(IReadOnlyList<string> players, Random random)
    {
        var shuffled = players.ToArray();
        random.Shuffle(shuffled);

        var blue = new List<string>();
        var red = new List<string>();
        for (var i = 0; i < shuffled.Length; i++)
            (i % 2 == 0 ? blue : red).Add(shuffled[i]);

        return (blue, red);
    }
}
