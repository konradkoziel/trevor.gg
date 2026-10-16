using trevor.Features.Teams;

namespace trevor.Tests;

public class TeamsCommandTests
{
    [Fact]
    public void ParsePlayers_TrimsAndDropsEmptyEntries()
    {
        Assert.Equal(["Konrad", "Janusz", "Jan"], TeamsCommand.ParsePlayers(" Konrad, ,Janusz,,Jan "));
        Assert.Empty(TeamsCommand.ParsePlayers(",,,"));
        Assert.Empty(TeamsCommand.ParsePlayers(null));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(10)]
    public void Split_KeepsEveryPlayer_AndBalancesSizes(int count)
    {
        var players = Enumerable.Range(1, count).Select(i => $"p{i}").ToArray();

        var (blue, red) = TeamsCommand.Split(players, new Random(42));

        Assert.Equal(players.Order(), blue.Concat(red).Order());
        Assert.InRange(blue.Count - red.Count, 0, 1);
    }

    [Fact]
    public async Task Execute_WithoutPlayers_ReturnsHint()
    {
        var text = await new TeamsCommand().ExecuteAsync(TestInteractions.Command("teams", """[{ "name": "gracze", "type": 3, "value": ",," }]"""));

        Assert.Contains("Podaj graczy", text);
    }

    [Fact]
    public async Task Execute_ListsAllPlayers()
    {
        var text = await new TeamsCommand().ExecuteAsync(TestInteractions.Command("teams", """[{ "name": "gracze", "type": 3, "value": "A,B,C,D" }]"""));

        Assert.All(["A", "B", "C", "D"], p => Assert.Contains($"• {p}", text));
    }
}
