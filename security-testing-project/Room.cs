namespace security_testing_project;

#region Class Summary
/// <summary>
/// This class defines a Room, the fundamental building block of the game world.
/// Each room has a name and description, and can contain items, a monster, and exits
/// to other rooms. It also includes properties for special conditions, like whether
/// it's a deadly trap or requires a key.
/// </summary>
#endregion

public class Room(string name, string description)
{
    public string Name { get; } = name;
    private string Description { get; set; } = description;
    public string? DescriptionWhenEmpty { get; init; }
    public string? DescriptionWhenMonsterDefeated { get; init; }

    public bool IsDeadly { get; init; }
    public bool RequiresKey { get; init; }
    public bool IsUnlocked { get; set; }
    
    public bool IsEncrypted { get; init; } 
    public string? EncryptedContentFile { get; init; }
    public string DecryptedContent { get; set; } = string.Empty;
    
    public List<Item> Items { get; } = new();
    public Monster? Monster { get; set; }
    public Dictionary<Direction, Room> Exits { get; } = new();

    public string Describe()
    {
        var lines = new List<string>
        {
            $"== {Name} =="
        };

        bool monsterDefeated = Monster is { IsAlive: false };

        if (IsEncrypted && !string.IsNullOrEmpty(DecryptedContent))
        {
            lines.Add(DecryptedContent);
        }

        else if (monsterDefeated && !string.IsNullOrEmpty(DescriptionWhenMonsterDefeated))
        {
            lines.Add(DescriptionWhenMonsterDefeated);
        }
        else if (Items.Count == 0 && !string.IsNullOrEmpty(DescriptionWhenEmpty))
        {
            lines.Add(DescriptionWhenEmpty);
        }
        else
        {
            lines.Add(Description);
        }

        if (IsDeadly) lines.Add("(A deadly presence lingers here)");
        if (RequiresKey && !IsUnlocked) lines.Add("The door is locked. You need a key.");
        
        if (Monster is { IsAlive: true })
        {
            lines.Add($"You sense danger: a {Monster.Name} is here.");
        }
        else if (monsterDefeated)
        {
            lines.Add($"The corpse of the defeated {Monster!.Name} is on the floor.");
        }
        
        if (Items.Count > 0) lines.Add("You see: " + string.Join(", ", Items.Select(i => i.Name)));
        if (Exits.Count > 0) lines.Add("Exits: " + string.Join(", ", Exits.Keys));
        return string.Join(Environment.NewLine, lines);
    }
}
