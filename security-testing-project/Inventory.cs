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
        return _items.Any(item => item.Type == key);
    }
}