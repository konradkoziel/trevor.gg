namespace trevor.Model.Response.cs;

public record DiscordResponse(
    int Type = 4,
    DiscordResponseData? Data = null
);