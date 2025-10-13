using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using trevor.Model;

namespace trevor
{
    public class CommandService
    {

        private readonly Command _cmd;
        private readonly DiscordInteraction _interaction;
        public CommandService(Command cmd, DiscordInteraction interaction)
        {
            _cmd = cmd;
            _interaction = interaction;

        }

        public string GetCommandResponse()
        {
            switch (_cmd)
            {
                case Command.ping:
                    return PingCommand();
                case Command.teams:
                    return TeamsCommand();
            }
            return "Unknown command";
        }

        private string PingCommand()
        {
            return "🏓 Ping? Ping?! Ty śmiesz mnie pingować? Ja tu siedzę od 3 godzin, analizuję trajektorie kul bilardowych i rozkład emocjonalny serwera, a ty mi wysyłasz ping? Dobrze. Pong. Ale wiedz, że obserwuję.";
        }

        private string TeamsCommand()
        {
            var raw = _interaction?.Data?.Options?.FirstOrDefault(o => o.Name == "gracze")?.Value;

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

            int max = Math.Max(team1.Count, team2.Count);
            var padded1 = team1.Concat(Enumerable.Repeat("", max - team1.Count)).ToList();
            var padded2 = team2.Concat(Enumerable.Repeat("", max - team2.Count)).ToList();

            var lines = Enumerable.Range(0, max)
                .Select(i => $"| {padded1[i],-12} | {padded2[i],-12} |");

            var table = string.Join("\n", lines);

            return $"""
                    🏆 **Wynik losowania drużyn**

                    🔵 **Niebiescy**
                    —————————————---
                    {string.Join("\n", team1.Select(p => $"• {p}"))}

                    🔴 **Czerwoni**
                    —————————————---
                    {string.Join("\n", team2.Select(p => $"• {p}"))}
                    """;
        }
    }
}
