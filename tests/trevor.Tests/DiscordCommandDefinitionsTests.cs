using System.Text.Json;
using trevor.Core;
using trevor.Features.Ask;
using trevor.Features.Lol.Rank;
using trevor.Features.Lol.Roast;
using trevor.Features.Ping;
using trevor.Features.Teams;

namespace trevor.Tests;

// discord/commands.json is what the deploy registers in Discord; keep it in sync with the code
public class DiscordCommandDefinitionsTests
{
    private static readonly ICommand[] Commands =
    [
        new PingCommand(),
        new TeamsCommand(),
        new AskCommand(null!),
        new RankCommand(null!, null!, null!),
        new RoastCommand(null!, null!, null!, null!),
    ];

    // Option names each command reads via GetOption
    private static readonly Dictionary<string, string[]> ExpectedOptions = new()
    {
        ["ping"] = [],
        ["teams"] = ["gracze"],
        ["ask"] = ["question"],
        ["rank"] = ["nick"],
        ["roast"] = ["nick"],
    };

    private static JsonElement[] Definitions()
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "commands.json")));
        return doc.RootElement.EnumerateArray().Select(e => e.Clone()).ToArray();
    }

    [Fact]
    public void EveryCommandInCode_IsRegistered_AndNothingElse()
    {
        var registered = Definitions().Select(d => d.GetProperty("name").GetString()).Order();

        Assert.Equal(Commands.Select(c => c.Name).Order(), registered);
    }

    [Fact]
    public void OptionNames_MatchWhatCommandsRead()
    {
        foreach (var definition in Definitions())
        {
            var name = definition.GetProperty("name").GetString()!;
            var options = definition.TryGetProperty("options", out var o)
                ? o.EnumerateArray().Select(x => x.GetProperty("name").GetString()).ToArray()
                : [];

            Assert.Equal(ExpectedOptions[name], options);
        }
    }

    [Fact]
    public void Definitions_FollowDiscordLimits()
    {
        foreach (var definition in Definitions())
        {
            var name = definition.GetProperty("name").GetString()!;
            Assert.Matches("^[a-z0-9_-]{1,32}$", name);
            Assert.InRange(definition.GetProperty("description").GetString()!.Length, 1, 100);
        }
    }
}
