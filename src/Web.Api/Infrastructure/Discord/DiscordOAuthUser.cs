using System.Text.Json.Serialization;

namespace Web.Api.Infrastructure.Discord;

// GET /users/@me with the "identify" scope: https://discord.com/developers/docs/resources/user#user-object
public record DiscordOAuthUser(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("global_name")] string? GlobalName,
    [property: JsonPropertyName("avatar")] string? Avatar)
{
    public string DisplayName => string.IsNullOrWhiteSpace(GlobalName) ? Username : GlobalName;
}
