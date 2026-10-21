using System.Text.Json.Serialization;

namespace trevor.Infrastructure.Discord.Models;

public record DiscordInteraction(
    [property: JsonPropertyName("type")] int Type,
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("application_id")] string ApplicationId,
    [property: JsonPropertyName("data")] InteractionData? Data,
    [property: JsonPropertyName("channel_id")] string? ChannelId,
    [property: JsonPropertyName("guild_id")] string? GuildId,
    // On a server Discord sends the caller as member.user, in a DM as user
    [property: JsonPropertyName("member")] InteractionMember? Member = null,
    [property: JsonPropertyName("user")] DiscordUser? User = null
);
