using System.Net;
using trevor.Features.Lol;
using trevor.Infrastructure.Riot;

namespace trevor.Tests;

public class RankCommandTests
{
    private sealed class FakeRiot : IRiotApiClient
    {
        public bool IsConfigured { get; init; } = true;
        public RiotAccount? Account { get; init; } = new("puuid-1", "Konrad", "EUNE");
        public IReadOnlyList<LeagueEntry> Entries { get; init; } = [];
        public Exception? Error { get; init; }
        public RiotId? RequestedId { get; private set; }

        public Task<RiotAccount?> GetAccountAsync(RiotId riotId, CancellationToken ct = default)
        {
            RequestedId = riotId;
            return Error is null ? Task.FromResult(Account) : Task.FromException<RiotAccount?>(Error);
        }

        public Task<IReadOnlyList<LeagueEntry>> GetLeagueEntriesAsync(string puuid, CancellationToken ct = default) =>
            Task.FromResult(Entries);
    }

    private static Task<string> Run(FakeRiot riot, string nick = "Konrad#EUNE") =>
        new RankCommand(riot).ExecuteAsync(
            TestInteractions.Command("rank", $$"""[{ "name": "nick", "type": 3, "value": "{{nick}}" }]"""));

    [Fact]
    public async Task WithoutApiKey_SaysSo()
    {
        Assert.Contains("klucza", await Run(new FakeRiot { IsConfigured = false }));
    }

    [Theory]
    [InlineData("Konrad")]
    [InlineData("#EUNE")]
    [InlineData("Konrad#")]
    public async Task NickWithoutTag_ReturnsHint(string nick)
    {
        Assert.Contains("z tagiem", await Run(new FakeRiot(), nick));
    }

    [Fact]
    public async Task UnknownPlayer_ReturnsNotFound()
    {
        Assert.Contains("Nie ma gracza **Ktoś#EUW**", await Run(new FakeRiot { Account = null }, "Ktoś#EUW"));
    }

    [Fact]
    public async Task PassesParsedRiotId()
    {
        var riot = new FakeRiot();
        await Run(riot, " Pan Kot #EUNE ");

        Assert.Equal(new RiotId("Pan Kot", "EUNE"), riot.RequestedId);
    }

    [Fact]
    public async Task Unranked_IsReported()
    {
        Assert.Contains("unranked", await Run(new FakeRiot()));
    }

    [Fact]
    public async Task SoloAndFlex_AreFormatted()
    {
        var text = await Run(new FakeRiot
        {
            Entries =
            [
                new LeagueEntry(LeagueEntry.FlexQueue, "SILVER", "I", 12, 10, 10),
                new LeagueEntry(LeagueEntry.SoloQueue, "GOLD", "II", 47, 58, 61),
            ]
        });

        Assert.Contains("🏅 **Konrad#EUNE**", text);
        Assert.Contains("Solo/Duo: **Gold II, 47 LP** — 58W / 61L (48.7%)", text);
        Assert.Contains("Flex: **Silver I, 12 LP** — 10W / 10L (50.0%)", text);
        Assert.Contains("idealnie przeciętny", text); // verdict follows Solo/Duo tier
    }

    [Fact]
    public async Task ApexTier_HasNoDivision()
    {
        var text = await Run(new FakeRiot { Entries = [new LeagueEntry(LeagueEntry.SoloQueue, "CHALLENGER", "I", 1200, 300, 200)] });

        Assert.Contains("**Challenger, 1200 LP**", text);
    }

    [Theory]
    [InlineData(HttpStatusCode.TooManyRequests, "limitem")]
    [InlineData(HttpStatusCode.Forbidden, "odrzucił mój klucz")]
    [InlineData(HttpStatusCode.Unauthorized, "odrzucił mój klucz")]
    public async Task RiotErrors_AreExplained(HttpStatusCode status, string expected)
    {
        Assert.Contains(expected, await Run(new FakeRiot { Error = new RiotApiException(status) }));
    }

    [Fact]
    public async Task OtherRiotErrors_Bubble_ToHandlerErrorMessage()
    {
        await Assert.ThrowsAsync<RiotApiException>(() =>
            Run(new FakeRiot { Error = new RiotApiException(HttpStatusCode.InternalServerError) }));
    }
}
