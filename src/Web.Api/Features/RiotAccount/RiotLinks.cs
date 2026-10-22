namespace Web.Api.Features.RiotAccount;

// Which of a user's accounts counts as the main one
public static class RiotLinks
{
    // A user can't spread across more accounts than this; keeps the leaderboard's Riot calls bounded
    public const int MaxAccounts = 5;

    // The flagged main account, else the first one linked (accounts from before multi-account linking have no flag)
    public static RiotLink? MainOf(IEnumerable<RiotLink> accounts)
    {
        var list = accounts.ToList();
        return list.FirstOrDefault(a => a.IsMain) ?? list.MinBy(a => a.LinkedAt);
    }

    // Main first, then in the order they were linked
    public static IReadOnlyList<RiotLink> Ordered(IEnumerable<RiotLink> accounts)
    {
        var list = accounts.ToList();
        var main = MainOf(list);
        return list.OrderBy(a => a == main ? 0 : 1).ThenBy(a => a.LinkedAt).ToList();
    }

    public static async Task<RiotLink?> GetMainAsync(this IRiotLinkStore store, string discordId, CancellationToken ct = default) =>
        MainOf(await store.ListAccountsAsync(discordId, ct));
}
