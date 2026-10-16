using trevor.Core;
using trevor.Infrastructure.Discord.Models;

namespace trevor.Features.Ping;

public class PingCommand : ICommand
{
    public string Name => "ping";
    public bool IsDeferred => false;

    public Task<string> ExecuteAsync(DiscordInteraction interaction, CancellationToken ct = default)
    {
        return Task.FromResult("🏓 Ping? Ping?! Ty śmiesz mnie pingować? Ja tu siedzę od 3 godzin, analizuję trajektorie kul bilardowych i rozkład emocjonalny serwera, a ty mi wysyłasz ping? Dobrze. Pong. Ale wiedz, że obserwuję.");
    }
}
