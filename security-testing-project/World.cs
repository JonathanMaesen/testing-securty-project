namespace security_testing_project;

public interface IGameworld
{
    string Look();
    string GetInventoryDescription();
    string Take(string itemName);
    string Go(Direction dir);
    string Fight();
    bool IsGameOver { get; }
    bool IsWin { get; }

}

public sealed class World : IGameworld
{
    private readonly Dictionary<string, Room> _rooms = new(StringComparer.OrdinalIgnoreCase);

    private Room Current { get; set; } = null!;
    private Player Player { get; } = new();
    public bool IsGameOver { get; private set; }
    public bool IsWin { get; private set; }
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

    public string Go(Direction dir)
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

            if (nextRoom.IsEncrypted && nextRoom.DecryptedContent == string.Empty)
            {
                string simulatedUserRole = "Player"; 
                
                string roomId = nextRoom.EncryptedContentFile?.Replace(".enc", "") ?? "unknown";
                string keyshare = GetSimulatedKeyShare(roomId, simulatedUserRole); 

                if (string.IsNullOrEmpty(keyshare))
                {
                    return "De kamer is versleuteld en u bent niet geautoriseerd om de keyshare op te halen (Rol te laag).";
                }

                string decryptionKey = CryptoHelper.GenerateDecryptionKey(keyshare);

                if (CryptoHelper.TryDecryptRoomContent(nextRoom.EncryptedContentFile!, decryptionKey, out var decryptedContent))
                {
                    nextRoom.DecryptedContent = decryptedContent;
                    Current = nextRoom;
                    return $"Succes! De kamerinhoud is ontsleuteld met de Keyshare.\n{Look()}";
                }
                else
                {
                    return "Decryptie mislukt. De sleutel is onjuist of het bestand is beschadigd.";
                }
            }
            else if (nextRoom.IsEncrypted && nextRoom.DecryptedContent != string.Empty)
            {
                 Current = nextRoom;
                 return Look();
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

    private string GetSimulatedKeyShare(string roomId, string userRole)
    {
        const string roomSecretShare = "SecretKeyShare123ForRoom1";
        const string roomAdminShare = "AdminOnlyKeyShare789ForRoom3";

        if (roomId.Equals("room_secret", StringComparison.OrdinalIgnoreCase))
        {
            return roomSecretShare;
        }

        if (roomId.Equals("room_admin", StringComparison.OrdinalIgnoreCase))
        {
            if (userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                return roomAdminShare;
            }
        }
        
        return string.Empty;
    }
}