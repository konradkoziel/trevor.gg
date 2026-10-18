using System.Text.Json;
using trevor.Infrastructure.Discord.Models;

namespace trevor.Infrastructure.Discord;

public static class InteractionExtensions
{
    public static string? GetOption(this DiscordInteraction interaction, string name)
    {
        var option = interaction.Data?.Options?.FirstOrDefault(o => o.Name == name);
        if (option is null)
            return null;

        return option.Value.ValueKind switch
        {
            JsonValueKind.String => option.Value.GetString(),
            JsonValueKind.Undefined or JsonValueKind.Null => null,
            _ => option.Value.GetRawText()
        };
    }
}
