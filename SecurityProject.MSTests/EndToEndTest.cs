using security_testing_project;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SecurityProject.MSTests
{
    [TestClass]
    public class EndToEndTest
    {
        private StringWriter _consoleOutput = null!;
        private World _world = null!;
        private CommandManager<string?> _terminal = null!;

        [TestInitialize]
        public void Setup()
        {
            _consoleOutput = new StringWriter();
            Console.SetOut(_consoleOutput);

            _world = CreateTestWorld();
            _terminal = SetupCommands(_world);
        }

        [TestMethod]
        public async Task FullPlaythrough_PlayerWins_WithKey()
        {
            await ExecuteCommand("look");
            await ExecuteCommand("go right");
            await ExecuteCommand("take key");
            await ExecuteCommand("go left");
            await ExecuteCommand("go up");

            var output = _consoleOutput.ToString();

            StringAssert.Contains(output, "You've found the treasure");
            Assert.IsTrue(_world.IsWin);
        }

        [TestMethod]
        public async Task EnterDeadlyPit_TriggersGameOver()
        {
            await ExecuteCommand("look");
            await ExecuteCommand("go left");

            var output = _consoleOutput.ToString();

            StringAssert.Contains(output, "You fall into a deadly pit and die.");
            Assert.IsTrue(_world.IsGameOver);
        }

        [TestMethod]
        public async Task InvalidDirection_ShowsUnknownMessage()
        {
            await ExecuteCommand("go diagonal");

            var output = _consoleOutput.ToString();
            StringAssert.Contains(output, "Unknown direction: diagonal");
        }

        [TestMethod]
        public async Task GoDown_GetSword_ThenFightGoblin_Succeeds()
        {
            // Player path:
            // 1. go down to Sword Room
            // 2. take sword
            // 3. go down again to Monster Lair
            // 4. fight goblin
            await ExecuteCommand("go down");
            await ExecuteCommand("take sword");
            await ExecuteCommand("go down");
            await ExecuteCommand("fight");

            var output = _consoleOutput.ToString();

            StringAssert.Contains(output, "goblin", "Expected goblin text not found.");
            StringAssert.Contains(output, "defeated", "Expected goblin defeated message missing.");
            Assert.IsFalse(_world.IsGameOver, "Game should not end after defeating goblin.");
        }

        // ---- Helpers ----

        private async Task ExecuteCommand(string command)
        {
            var parts = command.Split(' ', 2);
            var cmd = parts[0].ToLower();
            var arg = parts.Length > 1 ? parts[1] : null;
            await _terminal.TryCommand(cmd, arg);
        }

        private static CommandManager<string?> SetupCommands(World world)
        {
            var terminal = new CommandManager<string?>(() => Console.WriteLine(world.Look()));

            terminal.AddCommand("look", _ => { Console.WriteLine(world.Look()); return Task.FromResult(0); }, "Show world status.");
            terminal.AddCommand("inventory", _ => { Console.WriteLine(world.GetInventoryDescription()); return Task.FromResult(0); }, "Show inventory.");
            terminal.AddCommand("go", async arg =>
            {
                if (string.IsNullOrEmpty(arg))
                {
                    Console.WriteLine("Go where?");
                    return;
                }
                var dir = ParseDirection(arg);
                if (dir == null)
                {
                    Console.WriteLine($"Unknown direction: {arg}");
                    return;
                }
                Console.WriteLine(await world.Go(dir));
            }, "go <direction> - Move.");
            terminal.AddCommand("take", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                {
                    Console.WriteLine("Take what?");
                    return Task.FromResult(0);
                }
                Console.WriteLine(world.Take(arg));
                return Task.FromResult(0);
            }, "take <item> — Pick up an item.");
            terminal.AddCommand("fight", _ => { Console.WriteLine(world.Fight()); return Task.FromResult(0); }, "Fight the monster.");
            terminal.AddCommand("quit", _ => Task.FromResult(0), "Exit game.");

            return terminal;
        }

        private static Direction? ParseDirection(string input)
        {
            if (string.Equals(input, "up", StringComparison.OrdinalIgnoreCase)) return Direction.Up;
            if (string.Equals(input, "down", StringComparison.OrdinalIgnoreCase)) return Direction.Down;
            if (string.Equals(input, "left", StringComparison.OrdinalIgnoreCase)) return Direction.Left;
            if (string.Equals(input, "right", StringComparison.OrdinalIgnoreCase)) return Direction.Right;
            return null;
        }

        private static World CreateTestWorld()
        {
            var world = new World();

            world.AddRoom(new Room("Start", "You are in the center of a cave system. There are passages leading up, down, left, and right."));

            var deadlyPit = new Room("Deadly Pit", "A deep, dark pit. You didn't survive the fall.") { IsDeadly = true };
            world.AddRoom(deadlyPit);

            var keyRoom = new Room("Key Room", "You are in a small room. There is a shiny key on a pedestal.") { DescriptionWhenEmpty = "You are in a small room with an empty pedestal." };
            keyRoom.Items.Add(new Item("key", "A shiny key.", ItemType.Key));
            world.AddRoom(keyRoom);

            var treasureRoom = new Room("Treasure Room", "You've found the treasure! You win!") { RequiresKey = true };
            world.AddRoom(treasureRoom);

            var swordRoom = new Room("Sword Room", "This room has an old sword lying on the ground.") { DescriptionWhenEmpty = "This room is empty. The sword is gone." };
            swordRoom.Items.Add(new Item("sword", "A trusty sword.", ItemType.Weapon));
            world.AddRoom(swordRoom);

            var monsterRoom = new Room("Monster Lair", "A fearsome goblin blocks your path!")
            {
                Monster = new Monster("Goblin", true),
                DescriptionWhenMonsterDefeated = "The room is quiet now that the goblin has been defeated."
            };
            world.AddRoom(monsterRoom);

            // Connections
            world.Connect("Start", Direction.Left, "Deadly Pit");
            world.Connect("Start", Direction.Right, "Key Room");
            world.Connect("Key Room", Direction.Left, "Start");
            world.Connect("Start", Direction.Up, "Treasure Room");
            world.Connect("Treasure Room", Direction.Down, "Start");
            world.Connect("Start", Direction.Down, "Sword Room");
            world.Connect("Sword Room", Direction.Up, "Start");
            world.Connect("Sword Room", Direction.Down, "Monster Lair");
            world.Connect("Monster Lair", Direction.Up, "Sword Room");

            world.SetStart("Start");
            return world;
        }
    }
}
