namespace trevor.Infrastructure.Web;

// What the bot reads from the website: linked League accounts and saved builds
public interface IWebApiClient
{
    bool IsConfigured { get; }

    // Where people make builds and link accounts; null when not configured
    string? SiteUrl { get; }

    // Null when the user hasn't linked an account (or the site can't be reached)
    Task<LinkedPlayer?> GetLinkedPlayerAsync(string discordId, CancellationToken ct = default);

    // Accounts whose owners switched on match reports on the website; null when the site can't tell (not configured,
    // unreachable), which is not the same as nobody
    Task<IReadOnlyList<LinkedPlayer>?> GetTrackedPlayersAsync(CancellationToken ct = default);

    // The same switch as "Raporty na Discordzie" on the website, for /track and /untrack
    Task<WebTracking> SetTrackedAsync(string riotId, bool tracked, CancellationToken ct = default);

    // Newest first; null when the champion doesn't exist; empty when the user has no build for it
    Task<IReadOnlyList<WebBuildCard>?> GetBuildsAsync(string discordId, string champion, CancellationToken ct = default);
}
