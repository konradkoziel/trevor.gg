using System.Text.Json.Serialization;

namespace Web.Api.Infrastructure.Discord;

// GET /users/@me/guilds/{guild.id}/member: the user's nickname and server avatar, if they set them
public record DiscordGuildMember(
    [property: JsonPropertyName("nick")] string? Nick,
    [property: JsonPropertyName("avatar")] string? Avatar);
