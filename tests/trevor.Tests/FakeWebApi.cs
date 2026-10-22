using trevor.Infrastructure.Web;

namespace trevor.Tests;

// Not configured by default, like a bot without the website
public sealed class FakeWebApi : IWebApiClient
{
    public bool IsConfigured { get; init; }
    public string? SiteUrl { get; init; } = "https://site.test";
    public Dictionary<string, LinkedPlayer> Linked { get; } = new();
    // Null = the site couldn't answer
    public IReadOnlyList<LinkedPlayer>? Tracked { get; set; }
    // What SetTrackedAsync answers, and the calls it got
    public WebTracking Tracking { get; init; } = WebTracking.NotLinked;
    public List<(string RiotId, bool Tracked)> TrackingCalls { get; } = [];
    // Null = unknown champion
    public IReadOnlyList<WebBuildCard>? Builds { get; init; } = [];
    public (string DiscordId, string Champion)? RequestedBuilds { get; private set; }

    public Task<LinkedPlayer?> GetLinkedPlayerAsync(string discordId, CancellationToken ct = default) =>
        Task.FromResult(Linked.GetValueOrDefault(discordId));

    public Task<IReadOnlyList<LinkedPlayer>?> GetTrackedPlayersAsync(CancellationToken ct = default) => Task.FromResult(Tracked);

    public Task<WebTracking> SetTrackedAsync(string riotId, bool tracked, CancellationToken ct = default)
    {
        TrackingCalls.Add((riotId, tracked));
        return Task.FromResult(Tracking);
    }

    public Task<IReadOnlyList<WebBuildCard>?> GetBuildsAsync(string discordId, string champion, CancellationToken ct = default)
    {
        RequestedBuilds = (discordId, champion);
        return Task.FromResult(Builds);
    }
}
