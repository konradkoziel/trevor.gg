namespace trevor.Infrastructure.Discord.Models;

// https://discord.com/developers/docs/interactions/receiving-and-responding#interaction-response-object-interaction-callback-type
public static class ResponseType
{
    public const int Pong = 1;
    public const int ChannelMessage = 4;
    public const int DeferredChannelMessage = 5;
}
