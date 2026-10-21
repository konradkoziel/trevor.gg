namespace Web.Api.Infrastructure.Discord;

// https://discord.com/developers/docs/reference#image-formatting
public static class DiscordAvatars
{
    private const string CdnUrl = "https://cdn.discordapp.com";

    // Enough for the header badge, small to download
    private const int Size = 64;

    // Discord has six default avatars, picked from the user id for accounts without their own
    private const int DefaultAvatarCount = 6;

    // Snowflake ids keep their creation time above bit 22; the default avatar is chosen from that part
    private const int SnowflakeTimestampShift = 22;

    public static string Url(string userId, string? avatarHash)
    {
        if (!string.IsNullOrEmpty(avatarHash))
            return $"{CdnUrl}/avatars/{userId}/{avatarHash}.png?size={Size}";

        var index = ulong.TryParse(userId, out var id) ? (id >> SnowflakeTimestampShift) % DefaultAvatarCount : 0;
        return $"{CdnUrl}/embed/avatars/{index}.png";
    }
}
