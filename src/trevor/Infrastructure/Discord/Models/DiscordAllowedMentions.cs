using System.Text.Json.Serialization;

namespace trevor.Infrastructure.Discord.Models;

public record DiscordAllowedMentions(
    [property: JsonPropertyName("parse")] IReadOnlyList<string> Parse
)
{
    public static DiscordAllowedMentions None { get; } = new([]);
}
