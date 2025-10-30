namespace security_testing_project;

internal static class Program
{
    /// This class contains the main entry point of the game. It sets up the initial
    /// game state, defines the player commands, and runs the core loop that reads
    /// player input and updates the game world. It also handles the win and game-over
    /// conditions, including restarting the game on death.
    private static void Main()
    {
        var world = CreateTestWorld();
        var terminal = SetupCommands(world);

        Console.WriteLine(world.Look());
    
        while (true)
        {
            Console.Write("> ");
            var userInput = Console.ReadLine();
            if (string.IsNullOrEmpty(userInput)) continue;
        
            var parts = userInput.Split(' ', 2);
            var command = parts[0].ToLower();
            var arg = parts.Length > 1 ? parts[1] : null;

            if (command == "quit")
            {
                Console.WriteLine("Bye!");
                break;
            }
        
            Console.Clear();
            terminal.TryCommand(command, arg);
        
            if (world.IsWin)
            {
                Console.WriteLine("\nYou won!");
                break;
            }

            if (world.IsGameOver)
            {
                Console.WriteLine("\nGame Over. Restarting the game...");
                Thread.Sleep(2000); 
                world = CreateTestWorld();
                terminal = SetupCommands(world);
                Console.Clear();
                Console.WriteLine("A new adventure begins...");
                Console.WriteLine(world.Look());
            }
        }
    }

    private static CommandManager<string?> SetupCommands(World world)
    {
        var terminal = new CommandManager<string?>(() => Console.WriteLine(world.Look()));

        terminal.AddCommand("look", _ => Console.WriteLine(world.Look()), "Show the inventory, current room, items in the room, and exits.");
        terminal.AddCommand("inventory", _ => Console.WriteLine(world.GetInventoryDescription()), "Show only the inventory.");
        terminal.AddCommand("go", arg =>
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
            Console.WriteLine(world.Go(dir));
        }, "go <direction> - Move in the specified direction.");
        terminal.AddCommand("take", arg =>
        {
            if (string.IsNullOrEmpty(arg))
            {
                Console.WriteLine("Take what?");
                return;
            }
            Console.WriteLine(world.Take(arg));
        }, "take <item> — Pick up an item.");
        terminal.AddCommand("fight", _ => Console.WriteLine(world.Fight()), "Fight the monster if you are in the correct room.");
        terminal.AddCommand("quit", _ => {}, "Stop the game.");
        
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
    
        var deadlyPit = new Room("Deadly Pit", "A deep, dark pit. You didn't survive the fall.")
        {
            IsDeadly = true
        };
        world.AddRoom(deadlyPit);

        var keyRoom = new Room("Key Room", "You are in a small room. There is a shiny key on a pedestal.")
        {
            DescriptionWhenEmpty = "You are in a small room with an empty pedestal."
        };
        keyRoom.Items.Add(new Item("key", "A shiny key.", ItemType.Key));
        world.AddRoom(keyRoom);

        var treasureRoom = new Room("Treasure Room", "You've found the treasure! You win!")
        {
            RequiresKey = true
        };
        world.AddRoom(treasureRoom);

        var swordRoom = new Room("Sword Room", "This room has an old sword lying on the ground.")
        {
            DescriptionWhenEmpty = "This room is empty. The sword is gone."
        };
        swordRoom.Items.Add(new Item("sword", "A trusty sword.", ItemType.Weapon));
        world.AddRoom(swordRoom);

        var monsterRoom = new Room("Monster Lair", "A fearsome goblin blocks your path!")
        {
            Monster = new Monster("Goblin",true),
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