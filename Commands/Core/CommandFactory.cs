using OpenAI.Chat;

namespace trevor.Commands.Core
{
    public class CommandFactory(ChatClient chatClient) : ICommandFactory
    {
        public Task<ICommand> Create(string name) => name switch
        {
            "ping" => Task.FromResult<ICommand>(new PingCommand()),
            "teams" => Task.FromResult<ICommand>(new TeamsCommand()),
            "ask" => Task.FromResult<ICommand>(new AskCommand(chatClient)),
            _ => throw new NotSupportedException(name)
        };
    }
}