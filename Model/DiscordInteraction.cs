using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace trevor.Model
{
    public record DiscordInteraction(
    int Type,
    string Token,
    string Id,
    string ApplicationId,
    InteractionData Data,
    string ChannelId,
    string GuildId
    );
}
