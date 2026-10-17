using System.Text.Json.Serialization;

namespace trevor.Infrastructure.Discord.Models;

public record DiscordEmbedField(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("value")] string Value,
    [property: JsonPropertyName("inline")] bool Inline = false
)
{
    public const int MaxNameLength = 256;
    public const int MaxValueLength = 1024;

    public DiscordEmbedField Trimmed() => this with
    {
        Name = DiscordText.Truncate(Name, MaxNameLength),
        Value = DiscordText.Truncate(Value, MaxValueLength)
    };
}
