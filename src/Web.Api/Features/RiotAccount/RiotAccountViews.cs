using Lol.Shared.Riot;

namespace Web.Api.Features.RiotAccount;

public static class RiotAccountViews
{
    public static async Task<RiotChallengeView> ChallengeAsync(RiotLinkChallenge c, IDataDragon dataDragon, CancellationToken ct) =>
        new(c.RiotId, c.IconId, await dataDragon.ProfileIconUrlAsync(c.IconId, ct), c.ExpiresAt);
}
