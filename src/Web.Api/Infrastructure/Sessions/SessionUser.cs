namespace Web.Api.Infrastructure.Sessions;

// Who is logged in: the same Discord id the bot sees on the server
public record SessionUser(string DiscordId, string Name, string AvatarUrl);
