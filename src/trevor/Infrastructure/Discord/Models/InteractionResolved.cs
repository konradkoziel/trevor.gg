using System.Text.Json.Serialization;

namespace trevor.Infrastructure.Discord.Models;

// https://discord.com/developers/docs/interactions/receiving-and-responding#interaction-object-resolved-data-structure
public record InteractionResolved(
    [property: JsonPropertyName("messages")] IReadOnlyDictionary<string, DiscordMessage>? Messages = null
);
