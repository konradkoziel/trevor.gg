namespace trevor.Core;

// Every ICommand registered in DI is picked up here; adding a command doesn't touch this class
public class CommandRegistry(IEnumerable<ICommand> commands)
{
    private readonly Dictionary<string, ICommand> _commands =
        commands.ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);

    public ICommand? Find(string? name) =>
        name is not null && _commands.TryGetValue(name, out var command) ? command : null;

    public IReadOnlyCollection<string> Names => _commands.Keys;
}
