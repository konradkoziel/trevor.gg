using Web.Api.Infrastructure.Discord;

namespace Web.Api.Tests;

public class DiscordAvatarsTests
{
    [Fact]
    public void OwnAvatar_UsesItsHash()
    {
        Assert.Equal("https://cdn.discordapp.com/avatars/80351110224678912/a1b2c3.png?size=64",
            DiscordAvatars.Url("80351110224678912", "a1b2c3"));
    }

    [Fact]
    public void NoAvatar_UsesTheDefaultForTheId()
    {
        // (80351110224678912 >> 22) % 6 = 5
        Assert.Equal("https://cdn.discordapp.com/embed/avatars/5.png", DiscordAvatars.Url("80351110224678912", null));
        Assert.Matches(@"/embed/avatars/[0-5]\.png$", DiscordAvatars.Url("123456789012345678", null));
    }
}
