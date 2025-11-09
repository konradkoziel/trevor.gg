namespace trevor.Model.Response.cs;

public record DiscordResponse(
    int Type,
    DiscordResponseData? Data = null
);