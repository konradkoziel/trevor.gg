
namespace trevor.Commands.Core
{
    public interface ICommandFactory
    {
        public Task<ICommand> Create(string name);
    }
}
