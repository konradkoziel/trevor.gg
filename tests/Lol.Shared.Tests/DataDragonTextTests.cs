using Lol.Shared.Riot;

namespace Lol.Shared.Tests;

public class DataDragonTextTests
{
    [Fact]
    public void Tags_AreRemoved_AndBreaksBecomeLines()
    {
        var html = "<mainText><stats><attention>40</attention> obrażeń od ataku</stats><br><br><passive>Ostrze</passive><br>Zadaje &amp; leczy</mainText>";

        Assert.Equal("40 obrażeń od ataku\nOstrze\nZadaje & leczy", DataDragonText.Plain(html));
    }

    [Fact]
    public void Empty_StaysEmpty()
    {
        Assert.Equal("", DataDragonText.Plain(null));
        Assert.Equal("", DataDragonText.Plain("  "));
    }
}
