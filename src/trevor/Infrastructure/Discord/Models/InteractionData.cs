using System.Text.Json.Serialization;

namespace trevor.Infrastructure.Discord.Models;

public record InteractionData(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("options")] IReadOnlyList<InteractionOption>? Options,
    // Context-menu commands only: the clicked message's id, and the message itself in Resolved
    [property: JsonPropertyName("target_id")] string? TargetId = null,
    [property: JsonPropertyName("resolved")] InteractionResolved? Resolved = null
);
