using trevor.Features.Ask;

namespace trevor.Tests;

public class AskCommandTests
{
    private static async Task<string> Ask(FakeAi ai, string question) =>
        (await new AskCommand(ai).ExecuteAsync(
            TestInteractions.Command("ask", $$"""[{ "name": "question", "type": 3, "value": "{{question}}" }]"""))).Content!;

    [Fact]
    public async Task QuotesQuestion_AndAppendsAnswer()
    {
        var text = await Ask(new FakeAi("Ty."), "kto jest noobem?");

        Assert.Equal("**🧠 Pytanie:**\n> kto jest noobem?\nTy.", text);
    }

    [Fact]
    public async Task MultiLineQuestion_IsQuotedLineByLine()
    {
        var text = await Ask(new FakeAi("Nie."), "linia 1\\nlinia 2");

        Assert.Contains("> linia 1\n> linia 2", text);
    }

    [Fact]
    public async Task AiUnavailable_SaysSo()
    {
        Assert.Contains("Brak odpowiedzi", await Ask(new FakeAi(reply: null), "hej"));
    }

    [Fact]
    public async Task EmptyQuestion_DoesNotCallAi()
    {
        var ai = new FakeAi();

        Assert.Contains("Zadaj pytanie", await Ask(ai, " "));
        Assert.Equal(0, ai.Calls);
    }
}
