using System.Text.Json.Serialization;

namespace trevor.Infrastructure.Discord.Models;

// A message the bot sends: plain text, an embed, or both
public record DiscordResponseData(
    [property: JsonPropertyName("content")] string? Content = null,
    [property: JsonPropertyName("embeds")] IReadOnlyList<DiscordEmbed>? Embeds = null
)
{
    public const int MaxContentLength = 2000;

    // Never let user input or the model ping @everyone, @here, roles or users
    [JsonPropertyName("allowed_mentions")]
    public DiscordAllowedMentions AllowedMentions { get; init; } = DiscordAllowedMentions.None;

    public static DiscordResponseData FromText(string content) =>
        new(DiscordText.Truncate(content, MaxContentLength));

    public static DiscordResponseData FromEmbed(DiscordEmbed embed) =>
        new(Embeds: [embed.Trimmed()]);
}
