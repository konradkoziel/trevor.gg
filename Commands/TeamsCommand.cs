using trevor.Commands.Core;
using trevor.Model;

namespace trevor.Commands
{
    public class TeamsCommand : ICommand
    {
        public bool IsDefferedType => false;

        public Task<string> ExecuteAsync(DiscordInteraction interaction)
        {
            var raw = interaction?.Data?.Options?.FirstOrDefault(o => o.Name == "gracze")?.Value;

            var players = raw?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                             .Select(g => g.Trim())
                             .Where(g => !string.IsNullOrWhiteSpace(g))
                             .ToList();

            var rng = new Random();
            var przemieszani = players.OrderBy(_ => rng.Next()).ToList();

            var team1 = new List<string>();
            var team2 = new List<string>();

            for (int i = 0; i < przemieszani.Count; i++)
            {
                if (i % 2 == 0)
                    team1.Add(przemieszani[i]);
                else
                    team2.Add(przemieszani[i]);
            }

            var max = Math.Max(team1.Count, team2.Count);
            var padded1 = team1.Concat(Enumerable.Repeat("", max - team1.Count)).ToList();
            var padded2 = team2.Concat(Enumerable.Repeat("", max - team2.Count)).ToList();

            var lines = Enumerable.Range(0, max)
                .Select(i => $"| {padded1[i],-12} | {padded2[i],-12} |");

            var table = string.Join("\n", lines);

            return Task.FromResult($"""
                                    🏆 **Wynik losowania drużyn**

                                    🔵 **Niebiescy**
                                    —————————————---
                                    {string.Join("\n", team1.Select(p => $"• {p}"))}

                                    🔴 **Czerwoni**
                                    —————————————---
                                    {string.Join("\n", team2.Select(p => $"• {p}"))}
                                    """);
        }
    }
}
