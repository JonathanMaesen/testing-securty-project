using security_testing_project;
using System.Threading.Tasks;

namespace SecurityProject.MSTests
{
    [TestClass]
    public class TerminalTests // Renamed from ConsoleInterfacerTests
    {
        private StringWriter _consoleOutput = null!;

        [TestInitialize]
        public void Setup()
        {
            _consoleOutput = new StringWriter();
            Console.SetOut(_consoleOutput);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _consoleOutput.Dispose();
        }

        [TestMethod]
        public async Task TryCommand_ValidCommand_ExecutesAction()
        {
            var executed = false;
            var interfacer = new CommandManager<int>();
            interfacer.AddCommand("test", _ => {
                executed = true;
                return Task.FromResult(0);
            }, "A test command");

            await interfacer.TryCommand("test", 123);

            Assert.IsTrue(executed, "Command should set executed=true");
        }

        [TestMethod]
        public async Task TryCommand_UnknownCommand_ShowsErrorMessage()
        {
            var interfacer = new CommandManager<int>();

            await interfacer.TryCommand("nope");

            var output = _consoleOutput.ToString().Trim();
            StringAssert.Contains(output, "Unknown command: nope");
        }

        [TestMethod]
        public async Task HelpCommand_PrintsAllCommands()
        {
            var interfacer = new CommandManager<int>();
            interfacer.AddCommand("cmd1", _ => Task.FromResult(0), "desc1");
            interfacer.AddCommand("cmd2", _ => Task.FromResult(0), "desc2");

            await interfacer.TryCommand("help");

            var output = _consoleOutput.ToString();
            StringAssert.Contains(output, "cmd1");
            StringAssert.Contains(output, "cmd2");
        }

        [TestMethod]
        public void AddCommand_DuplicateName_ThrowsException() // This test does not need to be async
        {
            var interfacer = new CommandManager<int>();

            interfacer.AddCommand("test", _ => Task.FromResult(0), "desc");

            Assert.ThrowsException<InvalidOperationException>(() =>
                interfacer.AddCommand("test", _ => Task.FromResult(0), "desc2")
            );
        }

        [TestMethod]
        public async Task TryCommand_CommandThrows_PrintsErrorMessage()
        {
            const string command = "break";
            const string errorMessage = "this is an error";

            var interfacer = new CommandManager<int>();
            interfacer.AddCommand(command, _ => throw new Exception(errorMessage), "breaks");

            await interfacer.TryCommand(command);

            var output = _consoleOutput.ToString();
            StringAssert.Contains(output, $"Error executing command '{command}': {errorMessage}");
        }
    }
}
