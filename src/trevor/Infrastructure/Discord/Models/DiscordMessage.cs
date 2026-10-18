using System.Text.Json.Serialization;

namespace trevor.Infrastructure.Discord.Models;

// The fields we read from https://discord.com/developers/docs/resources/message#message-object
public record DiscordMessage(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("content")] string? Content,
    [property: JsonPropertyName("author")] DiscordUser? Author
);
