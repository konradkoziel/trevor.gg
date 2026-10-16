using System.Text.Json;
using System.Text.Json.Serialization;

namespace trevor.Infrastructure.Discord.Models;

// Value can be a string, number or bool depending on the option type, so keep it raw
public record InteractionOption(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("value")] JsonElement Value
);
