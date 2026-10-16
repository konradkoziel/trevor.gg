using System.Text.Json.Serialization;

namespace trevor.Infrastructure.Discord.Models;

public record DiscordResponse(
    [property: JsonPropertyName("type")] int Type = ResponseType.ChannelMessage,
    [property: JsonPropertyName("data")] DiscordResponseData? Data = null
)
{
    public static DiscordResponse Pong { get; } = new(ResponseType.Pong);
    public static DiscordResponse Deferred { get; } = new(ResponseType.DeferredChannelMessage);

    public static DiscordResponse Message(string content) =>
        new(ResponseType.ChannelMessage, DiscordResponseData.FromText(content));
}

// https://discord.com/developers/docs/interactions/receiving-and-responding#interaction-response-object-interaction-callback-type
public static class ResponseType
{
    public const int Pong = 1;
    public const int ChannelMessage = 4;
    public const int DeferredChannelMessage = 5;
}
