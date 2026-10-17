using System.Net;
using trevor.Features.Lol.Common;
using trevor.Infrastructure.Riot;

namespace trevor.Tests;

public class RiotPlayerRunnerTests
{
    private static Task<string> Run(FakeRiot riot, string nick = "Player#EUNE", Func<RiotPlayer, Task<string>>? action = null) =>
        new RiotPlayerRunner(riot).RunAsync(
            TestInteractions.Command("x", $$"""[{ "name": "nick", "type": 3, "value": "{{nick}}" }]"""),
            (player, _) => action?.Invoke(player) ?? Task.FromResult($"ok:{player.Puuid}:{player.DisplayName}"));

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
