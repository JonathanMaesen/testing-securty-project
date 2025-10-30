namespace security_testing_project;

public class PlayerInventory
{
    private readonly List<Item> _items = new();
    public void AddItem(Item item)
    {
        _items.Add(item);
    }

    public bool HasType(ItemType key)
    {
        return _items.Any(i => i.Type == key);
    }

    public string Describe()
    {
        if (_items.Count == 0)
        {
            return "Your inventory is empty.";
        }
        var itemNames = string.Join(", ", _items.Select(i => i.Name));
        return $"You are carrying: {itemNames}";
    }
}