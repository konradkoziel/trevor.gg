using trevor.Infrastructure.Discord.Models;

namespace trevor.Core;

// What the HTTP function sends back, and whether the interaction goes on the deferred queue
public record InteractionResult(DiscordResponse Response, bool EnqueueDeferred = false);
