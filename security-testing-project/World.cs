using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks; // Added for async operations

namespace security_testing_project;

public interface IGameworld
{
    string Look();
    string GetInventoryDescription();
    string Take(string itemName);
    Task<string> Go(Direction dir);
    string GoBack();
    string Fight();
    bool IsGameOver { get; }
    bool IsWin { get; }
}

public sealed class World : IGameworld
{
    private readonly Dictionary<string, Room> _rooms = new(StringComparer.OrdinalIgnoreCase);
    private readonly ApiService _apiService;

    private Room Current { get; set; } = null!;
    private Player Player { get; } = new();
    public bool IsGameOver { get; private set; }
    public bool IsWin { get; private set; }
    
    public World(ApiService apiService)
    {
        _apiService = apiService;
    }

    public void AddRoom(Room room) => _rooms[room.Name] = room;
    private Room Get(string name) => _rooms[name];
    public void Connect(string from, Direction direction, string to)
    {
        Room roomA = Get(from);
        Room roomB = Get(to);
        roomA.Exits[direction] = roomB;
    }

    public void SetStart(string roomName) => Current = Get(roomName);

    public string Look()
    {
        var roomDescription = Current.Describe();
        var inventoryDescription = Player.Inventory.Describe();
        return $"{roomDescription}{Environment.NewLine}{inventoryDescription}";
    }

    public string GetInventoryDescription() => Player.Inventory.Describe();

    public string Take(string itemName)
    {
        var item = Current.Items.FirstOrDefault(i => i.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase));
        if (item == null)
        {
            return $"There is no {itemName} here.";
        }

        Current.Items.Remove(item);
        Player.Inventory.AddItem(item);
        return $"You took the {item.Name}.\n{Look()}";
    }

    public async Task<string> Go(Direction dir)
    {
        if (Current.Monster is { IsAlive: true })
        {
            IsGameOver = true;
            return "The monster attacks you as you try to flee!";
        }
            
        if (Current.Exits.TryGetValue(dir, out var nextRoom))
        {
            if (nextRoom.IsDeadly)
            {
                IsGameOver = true;
                return "You fall into a deadly pit and die.";
            }

            Player.PreviousRoom = Current;

            if (nextRoom.IsEncrypted && string.IsNullOrEmpty(nextRoom.DecryptedContent))
            {
                Console.WriteLine($"\n[SECURITY] The room '{nextRoom.Name}' is encrypted.");
                
                // 1. Check API Connection / Login
                if (!_apiService.IsLoggedIn)
                {
                    return "Access Denied: You must be logged in to access encrypted rooms.";
                }

                // 2. Fetch Keyshare from API
                Console.WriteLine("Contacting API to retrieve Keyshare...");
                string? keyShare = await _apiService.GetKeyShareAsync(nextRoom.EncryptedContentFile!.Replace(".enc", "")); 
                // Note: Assuming roomId matches filename without extension, e.g., "room_secret"

                if (string.IsNullOrEmpty(keyShare))
                {
                    return "Access Denied: The API refused to give you the keyshare (Are you authorized?).";
                }

                // 3. Ask User for Passphrase
                Console.Write("Keyshare acquired. Enter your personal Passphrase: ");
                var passphrase = Console.ReadLine() ?? string.Empty;

                // 4. Derive the Key (Certificate Password)
                // Requirement: SHA256(keyshare + ":" + passphrase)
                var combinedSecret = $"{keyShare}:{passphrase}";
                var certPassword = ComputeSha256Hash(combinedSecret);
                
                // 5. Decrypt
                var encryptedFilePath = Path.Combine(AppContext.BaseDirectory, nextRoom.EncryptedContentFile!);
                // We reuse the existing cert logic, but the 'password' is now our derived hash
                // Note: You must assume the user has the .pfx file locally, protected by this specific hash.
                // For this assignment, we often just pass the path to the PFX.
                Console.Write("Enter path to your certificate file (.pfx): ");
                var certPath = Console.ReadLine() ?? string.Empty;

                if (CryptoHelper.TryDecryptRoomContentWithCert(encryptedFilePath, certPath, certPassword, out var decryptedContent))
                {
                    nextRoom.DecryptedContent = decryptedContent;
                    Current = nextRoom;
                    return $"Decrypt Success! The barrier dissolves.\n{Look()}";
                }
                else
                {
                    return $"Decryption Failed. (Error: {decryptedContent})";
                }
            }
            
            if (nextRoom is { RequiresKey: true, IsUnlocked: false })
            {
                if (Player.Inventory.HasType(ItemType.Key))
                {
                    nextRoom.IsUnlocked = true;
                    Current = nextRoom;

                    if (Current.Name == "Treasure Room")
                    {
                        IsWin = true;
                    }

                    return "You unlock the door with a key and enter.\n" + Look();
                }
                else
                {
                    return "The way is locked. You need a key.";
                }
            }
            Current = nextRoom;

            if (Current.Monster is { IsAlive: true })
            {
                return Fight();
            }

            if (Current.Name == "Treasure Room")
            {
                IsWin = true;
            }

            return Look();
        }
        return $"You can't go that way. \n{Look()}";
    }

    public string GoBack()
    {
        if (Player.PreviousRoom == null)
        {
            return "You can't go back.";
        }

        Current = Player.PreviousRoom;
        Player.PreviousRoom = null;
        return Look();
    }

    public string Fight()
    {
        if (Current.Monster == null)
            return "There is nothing to fight here.";

        if (!Current.Monster.IsAlive)
            return $"The {Current.Monster.Name} is already defeated.";
        if (!Player.Inventory.HasType(ItemType.Weapon))
        {
            IsGameOver = true;
            return $"A {Current.Monster.Name} attacks! You are defenseless without a weapon and die.";
        }
        Current.Monster.ReceiveDamage();
        return $"You fight the {Current.Monster.Name} and defeat it!\n{Look()}";
    }

    private static string ComputeSha256Hash(string rawData)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            return Convert.ToHexString(bytes); // Returns uppercase Hex
        }
    }
}