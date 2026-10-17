using System.Text.Json.Serialization;

namespace trevor.Infrastructure.Discord.Models;

public record DiscordResponseData(
    [property: JsonPropertyName("content")] string? Content = null
)
{
    public const int MaxContentLength = 2000;

    // Never let user input or the model ping @everyone, @here, roles or users
    [JsonPropertyName("allowed_mentions")]
    public DiscordAllowedMentions AllowedMentions { get; init; } = DiscordAllowedMentions.None;

    public static DiscordResponseData FromText(string content) =>
        new(content.Length <= MaxContentLength ? content : content[..(MaxContentLength - 1)] + "…");
}
