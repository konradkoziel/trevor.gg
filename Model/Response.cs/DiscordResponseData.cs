namespace trevor.Model.Response.cs;

public record DiscordResponseData(
    string? Content = null
    /*bool? Tts = null
    /*IEnumerable<DiscordEmbed>? Embeds = null,
    DiscordAllowedMentions? AllowedMentions = null,
    int? Flags = null, // np. 64 = ephemeral
    DiscordMessageComponent[]? Components = null#1#*/
);