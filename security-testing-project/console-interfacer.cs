using System.Threading.Tasks; // Added for async support

namespace security_testing_project
{ 
    public class CommandManager<T>
    {
        // Changed Action to Func<T, Task> to support async commands
        private readonly Dictionary<string, (Func<T, Task> Action, string Description)> _commands = new();
        private readonly Action? _onUnknownCommand; // Keeping this as Action for simplicity, it's not directly awaiting anything.

        public CommandManager(Action? onUnknownCommand = null)
        {
            _onUnknownCommand = onUnknownCommand;
            // Updated AddCommand for help to use Task.FromResult(0) for a synchronous Task
            AddCommand("help", _ =>
            {
                Console.WriteLine("\nAvailable commands:");
                foreach (var (name, (_, description)) in _commands.OrderBy(c => c.Key))
                {
                    var commandName = name.PadRight(10);
                    Console.WriteLine($"- {commandName}{description}");
                }
                return Task.FromResult(0);
            }, "Show a list of commands.");
        }
    
        // Changed to async Task to await command actions
        public async Task TryCommand(string command, T? value = default)
        {
            if (!_commands.TryGetValue(command, out var cmd))
            {
                Console.WriteLine($"Unknown command: {command}");
                _onUnknownCommand?.Invoke();
                return;
            }
            try
            {
                await cmd.Action(value!); // Await the async command action
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing command '{command}': {ex.Message}");
            }
        }

        // Changed Action to Func<T, Task>
        public void AddCommand(string name, Func<T, Task> action, string description)
        {
            if (_commands.ContainsKey(name))
                throw new InvalidOperationException($"Command '{name}' already exists.");

            _commands.Add(name, (action, description));
        }
    }
}

