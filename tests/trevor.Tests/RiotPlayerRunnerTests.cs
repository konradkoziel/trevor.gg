using System.Net;
using Lol.Shared.Riot.Models;
using Lol.Shared.Riot;
using trevor.Features.Lol.Common;
using trevor.Infrastructure.Discord.Models;
using trevor.Infrastructure.Web;

namespace trevor.Tests;

public class RiotPlayerRunnerTests
{
    private static async Task<string> Run(FakeRiot riot, string? nick = "Player#EUNE", Func<RiotPlayer, Task<string>>? action = null,
        FakeWebApi? web = null) =>
        (await new RiotPlayerRunner(riot, web ?? new FakeWebApi()).RunAsync(
            TestInteractions.Command("x", nick is null ? "[]" : $$"""[{ "name": "nick", "type": 3, "value": "{{nick}}" }]"""),
            async (player, _) => DiscordResponseData.FromText(
                await (action?.Invoke(player) ?? Task.FromResult($"ok:{player.Puuid}:{player.DisplayName}"))))).Content!;

    [Fact]
    public async Task NoNick_UsesTheAccountLinkedOnTheWebsite()
    {
        var riot = new FakeRiot { Account = new RiotAccount("p1", "Player", "EUNE") };
        var web = new FakeWebApi { IsConfigured = true };
        web.Linked["user-1"] = new LinkedPlayer("Player#EUNE", "site-puuid");

        Assert.Equal("ok:p1:Player#EUNE", await Run(riot, nick: null, web: web));
        // Looked up again by Riot ID: the site's PUUID may come from another API key
        Assert.Equal(new RiotId("Player", "EUNE"), riot.RequestedId);
    }

    [Fact]
    public async Task NoNick_NothingLinked_PointsToTheWebsite()
    {
        var web = new FakeWebApi { IsConfigured = true };

        Assert.Equal(LolMessages.NotLinked("https://site.test"), await Run(new FakeRiot(), nick: null, web: web));
    }

    [Fact]
    public async Task NoNick_WithoutTheWebsite_AsksForANick()
    {
        Assert.Equal(LolMessages.MissingTag, await Run(new FakeRiot(), nick: null));
    }

    [Fact]
    public async Task KnownPlayer_RunsAction_WithRiotsSpellingOfTheName()
    {
        var riot = new FakeRiot { Account = new RiotAccount("p1", "Pan Kot", "EUNE") };

        Assert.Equal("ok:p1:Pan Kot#EUNE", await Run(riot, " pan kot #eune "));
        Assert.Equal(new RiotId("pan kot", "eune"), riot.RequestedId);
    }

    [Fact]
    public async Task WithoutApiKey_SaysSo()
    {
        Assert.Equal(LolMessages.MissingKey, await Run(new FakeRiot { IsConfigured = false }));
    }

    [Theory]
    [InlineData("Player")]
    [InlineData("#EUNE")]
    [InlineData("Player#")]
    public async Task NickWithoutTag_ReturnsHint(string nick)
    {
        Assert.Equal(LolMessages.MissingTag, await Run(new FakeRiot(), nick));
    }

    [Fact]
    public async Task UnknownPlayer_ReturnsNotFound()
    {
        Assert.Equal(LolMessages.NotFound(new RiotId("Nobody", "EUW")),
            await Run(new FakeRiot { Account = null }, "Nobody#EUW"));
    }

    [Theory]
    [InlineData(HttpStatusCode.TooManyRequests, "limitem")]
    [InlineData(HttpStatusCode.Forbidden, "odrzucił mój klucz")]
    [InlineData(HttpStatusCode.Unauthorized, "odrzucił mój klucz")]
    public async Task RiotErrors_DuringLookup_AreExplained(HttpStatusCode status, string expected)
    {
        Assert.Contains(expected, await Run(new FakeRiot { Error = new RiotApiException(status) }));
    }

    [Fact]
    public async Task RiotErrors_InsideAction_AreExplainedToo()
    {
        var text = await Run(new FakeRiot(), action: _ => throw new RiotApiException(HttpStatusCode.TooManyRequests));

        Assert.Contains("limitem", text);
    }

    [Fact]
    public async Task OtherRiotErrors_Bubble_ToHandlerErrorMessage()
    {
        await Assert.ThrowsAsync<RiotApiException>(() =>
            Run(new FakeRiot { Error = new RiotApiException(HttpStatusCode.InternalServerError) }));
    }
}
