namespace security_testing_project
{ 
    public class CommandManager<T>
    {
        private readonly Dictionary<string, (Action<T> Action, string Description)> _commands = new();

        public CommandManager()
        {
            AddCommand("help", _ =>
            {
                Console.WriteLine("\nAvailable commands:");
                foreach (var (name, (_, description)) in _commands.OrderBy(c => c.Key))
                {
                    var commandName = name.PadRight(10);
                    Console.WriteLine($"- {commandName}{description}");
                }
            }, "Show a list of commands.");
        }
    
        public void TryCommand(string command, T? value = default)
        {
            if (!_commands.TryGetValue(command, out var cmd))
            {
                Console.WriteLine($"Unknown command: {command}");
                return;
            }
            try
            {
                cmd.Action(value);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing command '{command}': {ex.Message}");
            }
        }

        public void AddCommand(string name, Action<T> action, string description)
        {
            if (_commands.ContainsKey(name))
                throw new InvalidOperationException($"Command '{name}' already exists.");

            _commands.Add(name, (action, description));
        }
    }
}
