using System.Text.Json.Serialization;

namespace trevor.Model.Response.cs;

public record DiscordResponseData(
    string? Content = null
)
{
    // Never let user input or the model ping @everyone, @here, roles or users
    [JsonPropertyName("allowed_mentions")]
    public DiscordAllowedMentions AllowedMentions { get; init; } = DiscordAllowedMentions.None;
}

public record DiscordAllowedMentions(
    [property: JsonPropertyName("parse")] IReadOnlyList<string> Parse
)
{
    public static DiscordAllowedMentions None { get; } = new([]);
}
