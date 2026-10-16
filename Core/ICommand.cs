using trevor.Infrastructure.Discord.Models;

namespace trevor.Core;

public interface ICommand
{
    // Slash command name as registered in Discord, e.g. "ask"
    string Name { get; }

    // Slow commands (over Discord's 3 s limit) are acknowledged and run from the queue
    bool IsDeferred { get; }

    Task<string> ExecuteAsync(DiscordInteraction interaction, CancellationToken ct = default);
}
