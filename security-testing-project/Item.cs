namespace security_testing_project;

public enum ItemType { Misc, Key, Weapon }

public class Item(string name, string description, ItemType type = ItemType.Misc)
{
    public string Name { get; set; } = name;
    public ItemType Type { get; set; } = type;
    public string Description { get; set; } = description;
}