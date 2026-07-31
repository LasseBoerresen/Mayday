namespace RobotDomain.Motion;

public record WheelId(int Value) : ActuatorId
{
    public static readonly WheelId FrontLeft = new(0);
    public static readonly WheelId RearLeft = new(1);
    public static readonly WheelId FrontRight = new(2);
    public static readonly WheelId RearRight = new(3);
}