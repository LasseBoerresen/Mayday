namespace Dynamixel;

public record RotationDirection(uint Value)
{
    public static readonly RotationDirection Forward = new(0);
    public static readonly RotationDirection Reverse = new(1);

    public static RotationDirection FromDomain(
        RobotDomain.Structures.RotationDirection domainRotationDirection)
    {
        return domainRotationDirection switch
        {
            RobotDomain.Structures.RotationDirection.Forward => Forward,
            RobotDomain.Structures.RotationDirection.Reverse => Reverse,
            _ => throw new ArgumentOutOfRangeException(nameof(domainRotationDirection), domainRotationDirection, null)
        };
        ;
    }
}
