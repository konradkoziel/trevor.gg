namespace Web.Api.Infrastructure.Discord;

// Who logged in. Member: null when the user isn't on the configured server (or no server is configured)
public record DiscordIdentity(DiscordOAuthUser User, DiscordGuildMember? Member)
{
    public string DisplayName => string.IsNullOrWhiteSpace(Member?.Nick) ? User.DisplayName : Member.Nick;

    public string AvatarUrl(string? guildId) =>
        guildId is not null && !string.IsNullOrEmpty(Member?.Avatar)
            ? DiscordAvatars.GuildUrl(guildId, User.Id, Member.Avatar)
            : DiscordAvatars.Url(User.Id, User.Avatar);
}
