using System.Text.Json.Serialization;

namespace trevor.Infrastructure.Discord.Models;

public record DiscordEmbedFooter(
    [property: JsonPropertyName("text")] string Text
);
