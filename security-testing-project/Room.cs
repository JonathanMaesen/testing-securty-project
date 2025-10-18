using security_testing_project;
public sealed class Room
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
        if (Monster != null) lines.Add($"You sense danger: a {Monster.Name} is here.");
        if (Items.Count > 0) lines.Add("You see: " + string.Join(", ", Items.Select(i => i.Name)));
        if (Exits.Count > 0) lines.Add("Exits: " + string.Join(", ", Exits.Keys));
        return string.Join(Environment.NewLine, lines);
    }
}