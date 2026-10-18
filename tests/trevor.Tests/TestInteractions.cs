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

    // A message context-menu command used on a message by Ann (display name) / ann_123 (username)
    public static DiscordInteraction MessageCommand(string name, string? content, string? globalName = "Ann") =>
        Parse($$"""
                {
                  "type": 2, "id": "123", "application_id": "app", "token": "tok",
                  "channel_id": "chan", "guild_id": "guild",
                  "data": {
                    "id": "cmd", "name": "{{name}}", "type": 3, "target_id": "msg-1",
                    "resolved": { "messages": { "msg-1": {
                      "id": "msg-1", "content": {{JsonSerializer.Serialize(content)}},
                      "author": { "id": "user-1", "username": "ann_123", "global_name": {{JsonSerializer.Serialize(globalName)}} }
                    } } }
                  }
                }
                """);
}
