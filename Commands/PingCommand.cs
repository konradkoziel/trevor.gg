using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using trevor.Commands.Core;
using trevor.Model;

namespace trevor.Commands
{
    internal class PingCommand : ICommand
    {
        public async Task<string> ExecuteAsync(DiscordInteraction interaction)
        {
            return "🏓 Ping? Ping?! Ty śmiesz mnie pingować? Ja tu siedzę od 3 godzin, analizuję trajektorie kul bilardowych i rozkład emocjonalny serwera, a ty mi wysyłasz ping? Dobrze. Pong. Ale wiedz, że obserwuję.";
        }
    }
}
