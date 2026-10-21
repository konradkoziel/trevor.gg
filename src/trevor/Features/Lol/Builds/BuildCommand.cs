using trevor.Core;
using trevor.Infrastructure.Discord;
using trevor.Infrastructure.Discord.Models;
using trevor.Infrastructure.Web;

namespace trevor.Features.Lol.Builds;

// /build champion: — the caller's newest build for that champion, saved on the website
public class BuildCommand(IWebApiClient web) : ICommand
{
    public const string ChampionOption = "champion";

    public string Name => "build";

    // Asks the website, which may need a cold start
    public bool IsDeferred => true;

    public async Task<DiscordResponseData> ExecuteAsync(DiscordInteraction interaction, CancellationToken ct = default)
    {
        if (!web.IsConfigured)
            return DiscordResponseData.FromText(BuildMessage.SiteNotConnected);

        var champion = interaction.GetOption(ChampionOption)?.Trim();
        if (string.IsNullOrEmpty(champion) || interaction.GetUserId() is not { } userId)
            return DiscordResponseData.FromText(BuildMessage.MissingChampion);

        var cards = await web.GetBuildsAsync(userId, champion, ct);
        if (cards is null)
            return DiscordResponseData.FromText(BuildMessage.UnknownChampion(champion));
        if (cards.Count == 0)
            return DiscordResponseData.FromText(BuildMessage.NoBuild(champion, web.SiteUrl));

        return DiscordResponseData.FromEmbed(BuildMessage.Card(cards[0], cards.Count - 1));
    }
}
