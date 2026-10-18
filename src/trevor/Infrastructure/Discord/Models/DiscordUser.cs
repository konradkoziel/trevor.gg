using System.Text.Json.Serialization;

namespace trevor.Infrastructure.Discord.Models;

// https://discord.com/developers/docs/resources/user#user-object
public record DiscordUser(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("global_name")] string? GlobalName = null
)
{
    // The name people see on the server: display name, or the username when none is set
    public string DisplayName => GlobalName ?? Username;
}
