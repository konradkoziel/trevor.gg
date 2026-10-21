namespace Lol.Shared.Riot;

// Static game assets Data Dragon doesn't host, served by CommunityDragon's mirror of the game client
public static class CommunityDragon
{
    private const string RankEmblemBaseUrl =
        "https://raw.communitydragon.org/latest/plugins/rcp-fe-lol-static-assets/global/default/images/ranked-emblem";

    // League-V4 tier (e.g. "GOLD") → its ranked emblem image
    public static string RankEmblemUrl(string tier) => $"{RankEmblemBaseUrl}/emblem-{tier.ToLowerInvariant()}.png";
}
