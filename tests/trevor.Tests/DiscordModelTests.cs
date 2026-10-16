using System.Text.Json;
using trevor.Infrastructure.Discord;
using trevor.Infrastructure.Discord.Models;

namespace trevor.Tests;

public class DiscordModelTests
{
    [Fact]
    public void Message_SerializesWithEmptyAllowedMentions()
    {
        var json = JsonSerializer.Serialize(DiscordResponse.Message("@everyone hej"), DiscordJson.Options);

        Assert.Equal("""{"type":4,"data":{"content":"@everyone hej","allowed_mentions":{"parse":[]}}}""", json);
    }

    [Fact]
    public void Pong_SerializesAsType1Only()
    {
        Assert.Equal("""{"type":1}""", JsonSerializer.Serialize(DiscordResponse.Pong, DiscordJson.Options));
    }

    [Fact]
    public void LongContent_IsTruncatedToDiscordLimit()
    {
        var data = DiscordResponseData.FromText(new string('a', 5000));

        Assert.Equal(DiscordResponseData.MaxContentLength, data.Content!.Length);
        Assert.EndsWith("…", data.Content);
    }

    [Fact]
    public void Options_OfAnyType_AreReadAsText()
    {
        var interaction = TestInteractions.Command("x", """
            [
              { "name": "text", "type": 3, "value": "hello" },
              { "name": "count", "type": 4, "value": 5 },
              { "name": "flag", "type": 5, "value": true }
            ]
            """);

        Assert.Equal("hello", interaction.GetOption("text"));
        Assert.Equal("5", interaction.GetOption("count"));
        Assert.Equal("true", interaction.GetOption("flag"));
        Assert.Null(interaction.GetOption("missing"));
    }

    [Fact]
    public void Interaction_SurvivesQueueRoundTrip()
    {
        var original = TestInteractions.Command("ask", """[{ "name": "question", "type": 3, "value": "kto jest noobem?" }]""");

        var json = JsonSerializer.Serialize(original, DiscordJson.Options);
        var copy = JsonSerializer.Deserialize<DiscordInteraction>(json, DiscordJson.Options)!;

        Assert.Equal(original.Token, copy.Token);
        Assert.Equal(original.ApplicationId, copy.ApplicationId);
        Assert.Equal("ask", copy.Data?.Name);
        Assert.Equal("kto jest noobem?", copy.GetOption("question"));
    }
}
