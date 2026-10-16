using System.Text.Json;
using System.Text.Json.Serialization;

namespace trevor.Infrastructure.Discord;

public static class DiscordJson
{
    // Discord's API is snake_case; models also pin names with [JsonPropertyName]
    public static JsonSerializerOptions Options { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };
}
