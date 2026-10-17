using System.Text.Json.Serialization;

namespace trevor.Infrastructure.Discord.Models;

public record DiscordResponse(
    [property: JsonPropertyName("type")] int Type = ResponseType.ChannelMessage,
    [property: JsonPropertyName("data")] DiscordResponseData? Data = null
)
{
    public static DiscordResponse Pong { get; } = new(ResponseType.Pong);
    public static DiscordResponse Deferred { get; } = new(ResponseType.DeferredChannelMessage);

    public static DiscordResponse Message(DiscordResponseData data) =>
        new(ResponseType.ChannelMessage, data);

    public static DiscordResponse Message(string content) =>
        Message(DiscordResponseData.FromText(content));
}
