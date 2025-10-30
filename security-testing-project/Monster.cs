namespace security_testing_project;

public enum MonsterEnum
{
    Rat, Goblin, Skeleton, Troll, Vampire, Dragon
}
public class Monster
{
    public string Name { get; set; }
    public bool IsAlive { get; set; }
    public object MaxHp { get; set; }

    public Monster(string name, bool isAlive, int maxHp)
    {
        Name = name;
        IsAlive = isAlive;
        MaxHp = maxHp;
    }
    
    public void ReceiveDamage(int maxHp)
    {
        throw new NotImplementedException();
    }
    
}