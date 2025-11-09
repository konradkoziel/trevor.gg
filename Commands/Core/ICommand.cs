using trevor.Model;

namespace trevor.Commands.Core
{
    public interface ICommand
    {
        public bool IsDefferedType { get; } 
        public Task<string> ExecuteAsync(DiscordInteraction interaction);
    }
}
