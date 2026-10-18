using System.Text.Json.Serialization;

namespace trevor.Infrastructure.Discord.Models;

public record InteractionData(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("options")] IReadOnlyList<InteractionOption>? Options
);
