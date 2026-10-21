using System.Text.Json.Serialization;

namespace trevor.Infrastructure.Discord.Models;

// Who used the command on a server: https://discord.com/developers/docs/resources/guild#guild-member-object
public record InteractionMember([property: JsonPropertyName("user")] DiscordUser? User);
