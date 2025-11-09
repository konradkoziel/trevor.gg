using System.Text.Json.Serialization;

namespace trevor.Model
{
    public record DiscordInteraction(
        [property: JsonPropertyName("type")] int Type,
        [property: JsonPropertyName("token")] string Token,
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("application_id")] string ApplicationId,
        [property: JsonPropertyName("data")] InteractionData? Data,
        [property: JsonPropertyName("channel_id")] string? ChannelId,
        [property: JsonPropertyName("guild_id")] string? GuildId
    );
}
