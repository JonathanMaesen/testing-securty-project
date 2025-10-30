namespace security_testing_project;

public class Player
{
    public PlayerInventory Inventory { get; } = new();
    public bool IsAlive { get; set; } = true;
}