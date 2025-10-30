namespace security_testing_project;

public enum MonsterEnum
{
    Rat, Goblin, Skeleton, Troll, Vampire, Dragon
}
public class Monster(string name, bool isAlive)
{
    public string Name { get; set; } = name;
    public bool IsAlive { get; private set; } = isAlive;

    public void ReceiveDamage()
    {
        IsAlive = false;
    }
    
}