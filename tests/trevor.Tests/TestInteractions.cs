using System.Text.Json;
using trevor.Infrastructure.Discord;
using trevor.Infrastructure.Discord.Models;

namespace trevor.Tests;

// Builds interactions from JSON shaped like Discord's payloads
public static class TestInteractions
{
    public static DiscordInteraction Parse(string json) =>
        JsonSerializer.Deserialize<DiscordInteraction>(json, DiscordJson.Options)!;

    public static DiscordInteraction Ping() =>
        Parse("""{"type":1,"id":"1","application_id":"app","token":"tok","version":1}""");

    public static DiscordInteraction Command(string name, string optionsJson = "[]") =>
        Parse($$"""
                {
                  "type": 2, "id": "123", "application_id": "app", "token": "tok",
                  "channel_id": "chan", "guild_id": "guild",
                  "data": { "id": "cmd", "name": "{{name}}", "type": 1, "options": {{optionsJson}} }
                }
                """);
}
