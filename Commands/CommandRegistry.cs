using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace trevor.Commands
{
    public static class CommandRegistry
    {
        public static readonly Dictionary<string, Func<ICommand>> commands = new()
        {
            { "ping", () => new PingCommand() },
            { "teams" , () => new TeamsCommand() }
        };
    }
}
