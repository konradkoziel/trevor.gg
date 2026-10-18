using System.Text.Json.Serialization;

namespace trevor.Infrastructure.Discord.Models;

// https://discord.com/developers/docs/resources/message#embed-object
public record DiscordEmbed(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("description")] string? Description = null,
    [property: JsonPropertyName("color")] int? Color = null,
    [property: JsonPropertyName("fields")] IReadOnlyList<DiscordEmbedField>? Fields = null,
    [property: JsonPropertyName("thumbnail")] DiscordEmbedThumbnail? Thumbnail = null,
    [property: JsonPropertyName("footer")] DiscordEmbedFooter? Footer = null
)
{
    public const int MaxTitleLength = 256;
    public const int MaxDescriptionLength = 4096;

    // Applies Discord's limits so a long AI answer can't make the whole message fail
    public DiscordEmbed Trimmed() => this with
    {
        Title = DiscordText.Truncate(Title, MaxTitleLength),
        Description = Description is null ? null : DiscordText.Truncate(Description, MaxDescriptionLength),
        Fields = Fields?.Select(f => f.Trimmed()).ToList()
    };
}
