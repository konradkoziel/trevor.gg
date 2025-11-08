using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using trevor.Model;

namespace trevor.Commands.Core
{
    public class CommandFactory : ICommandFactory
    {
        private readonly ChatClient _chatClient;
        public CommandFactory(ChatClient chatClient) => _chatClient = chatClient;

        public async Task<ICommand> Create(string name) => name switch
        {
            "ping" => new PingCommand(),
            "teams" => new TeamsCommand(),
            "ask" => new AskCommand(_chatClient),
            _ => throw new NotSupportedException(name)
        };
    }
}
