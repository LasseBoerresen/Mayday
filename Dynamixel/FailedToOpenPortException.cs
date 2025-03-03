using LanguageExt.Common;

namespace Dynamixel;

public record FailedToOpenPortException : Expected
{
    public FailedToOpenPortException(PortNumber? portNumber) 
        : base(CreateMessage(portNumber), Code: 0) {}

    private static string CreateMessage(PortNumber? portNumber)
        => $"Failed to open port with: {portNumber}";
}
