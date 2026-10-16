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

            if (players is null || players.Count == 0)
                return Task.FromResult("Podaj graczy po przecinku, np. `Konrad,Janusz,Jan,Bartosz`.");

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
