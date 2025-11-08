using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using trevor.Model;

namespace trevor.Commands.Core
{
    public interface ICommand
    {
        public Task<string> ExecuteAsync(DiscordInteraction interaction);
    }
}
