namespace security_testing_project;

public enum ItemType { Misc, Key, Weapon }

public class Item
{
    public string Name { get; set; }
    public ItemType Type { get; set; }
    public string Description { get; set; }
    public Item(string name, string description, ItemType type = ItemType.Misc)
    {
        Name = name;
        Description = description;
    }
}