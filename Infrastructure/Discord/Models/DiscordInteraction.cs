using System.Text.Json.Serialization;

namespace trevor.Infrastructure.Discord.Models;

public record DiscordInteraction(
    [property: JsonPropertyName("type")] int Type,
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("application_id")] string ApplicationId,
    [property: JsonPropertyName("data")] InteractionData? Data,
    [property: JsonPropertyName("channel_id")] string? ChannelId,
    [property: JsonPropertyName("guild_id")] string? GuildId
);

// https://discord.com/developers/docs/interactions/receiving-and-responding#interaction-object-interaction-type
public static class InteractionType
{
    public const int Ping = 1;
    public const int ApplicationCommand = 2;
}
