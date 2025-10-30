using security_testing_project;
namespace SecurityProject.MSTests
{
    [TestClass]
    public class ConsoleInterfacerTests
    {
        private StringWriter _consoleOutput;

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
        public void TryCommand_ValidCommand_ExecutesAction()
        {
            var executed = false;
            var interfacer = new CommandManager<int>();
            interfacer.AddCommand("test", _ => executed = true, "A test command");

            interfacer.TryCommand("test", 123);

            Assert.IsTrue(executed, "Command should set executed=true");
        }

        [TestMethod]
        public void TryCommand_UnknownCommand_ShowsErrorMessage()
        {
            var interfacer = new CommandManager<int>();

            interfacer.TryCommand("nope");

            var output = _consoleOutput.ToString().Trim();
            StringAssert.Contains(output, "Unknown command: nope");
        }

        [TestMethod]
        public void HelpCommand_PrintsAllCommands()
        {
            var interfacer = new CommandManager<int>();
            interfacer.AddCommand("cmd1", _ => {}, "desc1");
            interfacer.AddCommand("cmd2", _ => {}, "desc2");

            interfacer.TryCommand("help");

            var output = _consoleOutput.ToString();
            StringAssert.Contains(output, "cmd1");
            StringAssert.Contains(output, "cmd2");
        }

        [TestMethod]
        public void AddCommand_DuplicateName_ThrowsException()
        {
            var interfacer = new CommandManager<int>();

            interfacer.AddCommand("test", _ => { }, "desc");

            Assert.ThrowsException<InvalidOperationException>(() =>
                interfacer.AddCommand("test", _ => { }, "desc2")
            );
        }

        [TestMethod]
        public void TryCommand_CommandThrows_PrintsErrorMessage()
        {
            const string command = "break";
            const string errorMessage = "this is an error";

            var interfacer = new CommandManager<int>();
            interfacer.AddCommand(command, _ => throw new Exception(errorMessage), "breaks");

            interfacer.TryCommand(command);

            var output = _consoleOutput.ToString();
            StringAssert.Contains(output, $"Error executing command '{command}': {errorMessage}");
        }
    }
}
