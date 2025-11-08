using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using trevor.Model;

namespace trevor.Commands
{
    public class CommandFactory
    {
        public async Task<ICommand> Create(string commandName, DiscordInteraction interaction)
        {
            if (CommandRegistry.commands.TryGetValue(commandName, out var command))
            {
                return command();
            }
            throw new ArgumentException("Uknown command");
        }
    }
}
