using System.Threading.Tasks; // Added for async Main

namespace security_testing_project;

internal static class Program
{
    /// This class contains the main entry point of the game. It sets up the initial
    /// game state, defines the player commands, and runs the core loop that reads
    /// player input and updates the game world. It also handles the win and game-over
    /// conditions, including restarting the game on death.
    private static async Task Main()
    {
        while (!ApiService.IsLoggedIn)
        {
            Console.Clear();
            Console.WriteLine("Welcome to the Secure Text Adventure!");
            Console.WriteLine("Please log in to continue.");
            Console.Write("Username: ");
            var username = Console.ReadLine();
            Console.Write("Password: ");
            var password = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("Username and password cannot be empty. Press any key to try again.");
                Console.ReadKey();
                continue;
            }

            var (success, message) = await ApiService.LoginAsync(username, password);

            if (success)
            {
                Console.WriteLine($"Login successful! Welcome, {ApiService.Username} (Role: {ApiService.Role}). Press any key to start your adventure.");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine($"Login failed: {message}. Press any key to try again.");
                Console.ReadKey();
            }
        }

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
            await terminal.TryCommand(command, arg); // Await the async command
        
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

        terminal.AddCommand("look", _ => { Console.WriteLine(world.Look()); return Task.FromResult(0); }, "Show the inventory, current room, items in the room, and exits.");
        terminal.AddCommand("inventory", _ => { Console.WriteLine(world.GetInventoryDescription()); return Task.FromResult(0); }, "Show only the inventory.");
        terminal.AddCommand("go", async arg => // Made async
        {
            if (string.IsNullOrEmpty(arg))
            {
                Console.WriteLine("Go where?");
                return; // Returns Task.CompletedTask implicitly
            }
            var dir = ParseDirection(arg);
            if (dir == null)
            {
                Console.WriteLine($"Unknown direction: {arg}");
                return; // Returns Task.CompletedTask implicitly
            }
            Console.WriteLine(await world.Go(dir)); // Await world.Go
        }, "go <direction> - Move in the specified direction.");
        terminal.AddCommand("take", arg => {
            if (string.IsNullOrEmpty(arg))
            {
                Console.WriteLine("Take what?");
                return Task.FromResult(0);
            }
            Console.WriteLine(world.Take(arg));
            return Task.FromResult(0);
        }, "take <item> — Pick up an item.");
        terminal.AddCommand("fight", _ => { Console.WriteLine(world.Fight()); return Task.FromResult(0); }, "Fight the monster if you are in the correct room.");
        terminal.AddCommand("quit", _ => Task.FromResult(0), "Stop the game.");
        
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
    
        var secretRoom = new Room("Secret Vault", "U staat voor een massieve stalen deur. Terminal: 'Gecodeerde inhoud vereist KeyShare en Passphrase voor decryptie van room_secret.enc'.")
        {
            IsEncrypted = true,
            EncryptedContentFile = "room_secret.enc",
        };
        world.AddRoom(secretRoom);
        
        var adminRoom = new Room("Admin Sanctum", "U staat voor een ondoordringbare zwarte monoliet. Toegang geweigerd. Terminal: 'Gecodeerde inhoud vereist Admin-rol en KeyShare voor room_admin.enc'.")
        {
            IsEncrypted = true,
            EncryptedContentFile = "room_admin.enc",
        };
        world.AddRoom(adminRoom);
        
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
        
        world.Connect("Sword Room", Direction.Left, "Secret Vault");
        world.Connect("Secret Vault", Direction.Right, "Sword Room");
    
        world.Connect("Treasure Room", Direction.Left, "Admin Sanctum");
        world.Connect("Admin Sanctum", Direction.Right, "Treasure Room");

        world.SetStart("Start");
        return world;
    }
}