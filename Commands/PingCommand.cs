using trevor.Commands.Core;
using trevor.Model;

namespace trevor.Commands
{
    public class PingCommand : ICommand
    {
        public bool IsDefferedType => false;
        
        public Task<string> ExecuteAsync(DiscordInteraction interaction)
        {
            return Task.FromResult("🏓 Ping? Ping?! Ty śmiesz mnie pingować? Ja tu siedzę od 3 godzin, analizuję trajektorie kul bilardowych i rozkład emocjonalny serwera, a ty mi wysyłasz ping? Dobrze. Pong. Ale wiedz, że obserwuję.");
        }
    }
}
