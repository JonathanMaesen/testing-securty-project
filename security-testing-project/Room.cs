namespace security_testing_project;

#region Class Summary
/// <summary>
/// This class defines a Room, the fundamental building block of the game world.
/// Each room has a name and description, and can contain items, a monster, and exits
/// to other rooms. It also includes properties for special conditions, like whether
/// it's a deadly trap or requires a key.
/// </summary>
#endregion

public class Room
{
    public string Name { get; }
    public string Description { get; set; }
    public bool IsDeadly { get; set; }
    public bool RequiresKey { get; set; }
    public bool IsUnlocked { get; set; }
    public List<Item> Items { get; } = new();
    public Monster? Monster { get; set; }
    public Dictionary<Direction, Room> Exits { get; } = new();
    public Room(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public string Describe()
    {
        var lines = new List<string>
        {
            $"== {Name} ==",
            Description
        };
        if (IsDeadly) lines.Add("(A deadly presence lingers here.)");
        if (RequiresKey && !IsUnlocked) lines.Add("The door is locked. You need a key.");
        if (Monster is { IsAlive: true }) lines.Add($"You sense danger: a {Monster.Name} is here.");
        if (Items.Count > 0) lines.Add("You see: " + string.Join(", ", Items.Select(i => i.Name)));
        if (Exits.Count > 0) lines.Add("Exits: " + string.Join(", ", Exits.Keys));
        return string.Join(Environment.NewLine, lines);
    }
}
