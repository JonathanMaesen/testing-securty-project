namespace security_testing_project;

public enum DirectionEnum { Up, Left, Down, Right }

public sealed class Direction
{
    public static readonly Direction Up = new(DirectionEnum.Up);
    public static readonly Direction Down = new(DirectionEnum.Down);
    public static readonly Direction Left = new(DirectionEnum.Left);
    public static readonly Direction Right = new(DirectionEnum.Right);

    public DirectionEnum Value { get; }
    private Direction(DirectionEnum value) => Value = value;
}
