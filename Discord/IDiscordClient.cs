using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace trevor.Discord
{
    public interface IDiscordClient
    {
        public Task SendMessageAsync(ulong channelId, string message);
    }
}
