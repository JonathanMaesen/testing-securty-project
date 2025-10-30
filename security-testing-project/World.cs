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
        return $"You took the {item.Name}.";
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

            if (Current.Name == "Treasure Room")
            {
                IsWin = true;
            }

            return Look();
        }
        return "You can't go that way.";
    }

    public string Fight()
    {
        throw new NotImplementedException();
    }
}