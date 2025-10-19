using System;
using System.Collections.Generic;

namespace security_testing_project
{
    public class Consoleinterfacer<T>
    {
        private Dictionary<string, Action<T>> Commands { get; set; }

        private Dictionary<string, string> Commandswithdescription { get; set; }

        public Consoleinterfacer(Dictionary<string, Action<T>> list, Dictionary<string, string> listwithdescription)
        {
            Commands = new Dictionary<string, Action<T>>(list);
            Commandswithdescription = new(listwithdescription);
            foreach (var item in Commands)
            {
                if (!Commandswithdescription.TryGetValue(item.Key, out var f)) 
                {
                    throw new Exception($"Function with {item.Key} not found in list with descriptions");
                }
            }
        }

        public void TryCommand(string command, T value)
        {
            if (!Commands.TryGetValue(command, out var f))
            {
                Console.WriteLine($"Unknown command: {command}");
                return;
            }
            try
            {
                f(value);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing command '{command}': {ex.Message}");
            }
        }

        public void AddCommand(string name, Action<T> action, string description)
        {
            if (Commands.ContainsKey(name))
                throw new InvalidOperationException($"Command '{name}' already exists.");

            Commands.Add(name, action);
        }
    }
}
