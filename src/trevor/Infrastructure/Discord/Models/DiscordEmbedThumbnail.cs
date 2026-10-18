using System.Text.Json.Serialization;

namespace trevor.Infrastructure.Discord.Models;

public record DiscordEmbedThumbnail(
    [property: JsonPropertyName("url")] string Url
);
