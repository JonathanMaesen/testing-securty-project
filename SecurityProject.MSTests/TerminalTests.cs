using security_testing_project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecurityProject.MSTests
{
    [TestClass]
    public class ConsoleInterfacerTests
    {
        private StringWriter consoleOutput;

        [TestInitialize]
        public void Setup()
        {
            consoleOutput = new StringWriter();
            Console.SetOut(consoleOutput);
        }

        [TestCleanup]
        public void Cleanup()
        {
            consoleOutput.Dispose();
        }

        [TestMethod]
        public void TryCommand_ValidCommand_ExecutesAction()
        {
            bool executed = false;
            var cmds = new Dictionary<string, Action<int>>
            {
                { "test", (i) => executed = true }
            };
            var interfacer = new Consoleinterfacer<int>(cmds);

            interfacer.TryCommand("test", 123);

            Assert.IsTrue(executed, "Command should set executed=true");
        }

        [TestMethod]
        public void TryCommand_UnknownCommand_ShowsErrorMessage()
        {
            var cmds = new Dictionary<string, Action<int>>();
            var interfacer = new Consoleinterfacer<int>(cmds);

            interfacer.TryCommand("nope");

            var output = consoleOutput.ToString().Trim();
            StringAssert.Contains(output, "Unknown command: nope");
        }

        [TestMethod]
        public void HelpCommand_PrintsAllCommands()
        {
            var cmds = new Dictionary<string, Action<int>>
            {
                { "cmd1", (x)=>{} },
                { "cmd2", (x)=>{} }
            };
            var interfacer = new Consoleinterfacer<int>(cmds);

            interfacer.TryCommand("help", 0);

            var output = consoleOutput.ToString();
            StringAssert.Contains(output, "cmd1");
            StringAssert.Contains(output, "cmd2");
        }

        [TestMethod]
        public void AddCommand_DuplicateName_ThrowsException()
        {
            var cmds = new Dictionary<string, Action<int>>();
            var interfacer = new Consoleinterfacer<int>(cmds);

            interfacer.AddCommand("test", (i) => { }, "desc");

            Assert.ThrowsException<InvalidOperationException>(() =>
                interfacer.AddCommand("test", (i) => { }, "desc2")
            );
        }

        [TestMethod]
        public void TryCommand_CommandThrows_PrintsErrorMessage()
        {

            string command = "break";
            string errormessage = "this is an error";

            var cmds = new Dictionary<string, Action<int>>
            {
                { command, (i) => throw new Exception(errormessage) }
            };
            var interfacer = new Consoleinterfacer<int>(cmds);

            interfacer.TryCommand(command);

            var output = consoleOutput.ToString();
            StringAssert.Contains(output, $"Error executing command '{command}': {errormessage}");
        }
    }
}
