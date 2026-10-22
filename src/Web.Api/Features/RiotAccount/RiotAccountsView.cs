namespace Web.Api.Features.RiotAccount;

// GET /api/me/riot and the answer to every change: the user's accounts, main first
public record RiotAccountsView(IReadOnlyList<RiotAccountView> Accounts, int MaxAccounts)
{
    public static RiotAccountsView Of(IEnumerable<RiotLink> accounts)
    {
        var ordered = RiotLinks.Ordered(accounts);
        var main = RiotLinks.MainOf(ordered);
        return new RiotAccountsView(ordered.Select(a => new RiotAccountView(a.Puuid, a.RiotId, a == main, a.Tracked)).ToList(), RiotLinks.MaxAccounts);
    }
}
