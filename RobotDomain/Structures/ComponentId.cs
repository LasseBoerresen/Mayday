namespace RobotDomain.Structures;

public record ComponentId(Guid Value)
{
    public static ComponentId New => new(Guid.NewGuid());
}
