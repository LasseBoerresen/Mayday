namespace Dynamixel;

public record PortNumber(int Value)
{
    public bool PortIsOpen => Value != 0;
};
