namespace security_testing_project;

class Program
{
    static void Main(string[] args)
    {
        Dictionary<string, Action<char>> commands = new Dictionary<string, Action<char>>();

        commands.Add("exit", (char v) => { Console.WriteLine("Bye!"); });

        Consoleinterfacer<char> terminal = new(commands);

        bool continueloop = true;
        while (continueloop)
        {
            Console.Write("> ");
            string userinput = Console.ReadLine();
            string[] spilts = userinput.Split(" ");
            string command = spilts[0];
            if (command == "exit") 
            {
                continueloop = false;
            }
            terminal.TryCommand(command);
        }
    }
}