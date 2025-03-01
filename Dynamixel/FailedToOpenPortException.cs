namespace Dynamixel;

public class FailedToOpenPortException : Exception
{
    public FailedToOpenPortException(PortNumber? portNumber) 
        : base(Message(portNumber)) {}

    private static string Message(PortNumber? portNumber)
        => $"Failed to open port with: {portNumber}";
}
