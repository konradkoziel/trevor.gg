using trevor.Features.RateThis;
using trevor.Infrastructure.OpenAi;

namespace trevor.Tests;

public class RateThisCommandTests
{
    private static async Task<string> Rate(FakeAi ai, string? content, string? globalName = "Ann") =>
        (await new RateThisCommand(ai).ExecuteAsync(TestInteractions.MessageCommand("Rate this", content, globalName))).Content!;

    [Fact]
    public async Task QuotesMessage_AndAppendsVerdict()
    {
        var text = await Rate(new FakeAi("3/10. Nawet nie śmieszne."), "idę na mida\nnie dyskutuj");

        Assert.Equal("💬 **Ann:**\n> idę na mida\n> nie dyskutuj\n3/10. Nawet nie śmieszne.", text);
    }

    [Fact]
    public async Task AiGetsAuthorAndMessage_WithVerdictInstruction()
    {
        var ai = new FakeAi();
        await Rate(ai, "gg ez");

        Assert.Equal(TrevorPrompts.MessageVerdict, ai.LastInstruction);
        Assert.Equal("Autor: Ann\nWiadomość:\ngg ez", ai.LastInput);
    }

    [Fact]
    public async Task NoDisplayName_UsesUsername()
    {
        Assert.StartsWith("💬 **ann_123:**", await Rate(new FakeAi(), "gg ez", globalName: null));
    }

    [Fact]
    public async Task AiUnavailable_FallsBackToCannedVerdict()
    {
        Assert.EndsWith(RateThisMessage.FallbackVerdict, await Rate(new FakeAi(reply: null), "gg ez"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task MessageWithoutText_IsNotSentToAi(string? content)
    {
        var ai = new FakeAi();

        Assert.Equal(RateThisMessage.NothingToRate, await Rate(ai, content));
        Assert.Equal(0, ai.Calls);
    }

    [Fact]
    public async Task LongMessage_IsCutInTheQuote_ButVerdictStays()
    {
        var text = await Rate(new FakeAi("Za długie, nie czytam."), new string('a', 3000));

        Assert.True(text.Length < 2000);
        Assert.EndsWith("Za długie, nie czytam.", text);
    }

    [Fact]
    public async Task SlashCommandWithoutTarget_IsNotSentToAi()
    {
        var ai = new FakeAi();

        var reply = await new RateThisCommand(ai).ExecuteAsync(TestInteractions.Command("Rate this"));

        Assert.Equal(RateThisMessage.NothingToRate, reply.Content);
        Assert.Equal(0, ai.Calls);
    }
}
