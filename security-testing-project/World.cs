namespace security_testing_project;

public sealed class World
{
    private readonly Dictionary<string, Room> _rooms = new(StringComparer.OrdinalIgnoreCase);

    private Room Current { get; set; } = null!;
    public Player Player { get; } = new();

    public bool IsGameOver { get; private set; }
    public bool IsWin { get; private set; }

    public void AddRoom(Room r) => _rooms[r.Name] = r;
    private Room Get(string name) => _rooms[name];

    public void Connect(string from, Direction direction, string to)
    {
        var a = Get(from);
        var b = Get(to);
        a.Exits[direction] = b;
    }

    public void SetStart(string roomName) => Current = Get(roomName);

    public string Look() => Current.Describe();

    public string Take(string itemName)
    {
        throw new NotImplementedException();
    }

    public string Go(Direction dir)
    {
        throw new NotImplementedException();
    }

    public string Fight()
    {
        throw new NotImplementedException();
    }
}