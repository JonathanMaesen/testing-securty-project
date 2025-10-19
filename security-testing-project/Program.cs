namespace security_testing_project;

class Program
{
    static void Main(string[] args)
    {
        Dictionary<string, Action<string[]>> commands = new Dictionary<string, Action<string[]>>();
        //commands.Add("what", void (string[] v) => { Console.WriteLine("Yes"); });
        Consoleinterfacer<string[]> terminal = new(commands);

        while (true)
        {
            Console.Write("> ");
            string userinput = Console.ReadLine();
            string[] spilts = userinput.Split(" ");
            string command = spilts[0];
            terminal.TryCommand(command, []);
        }
    }
}