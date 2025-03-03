using LanguageExt.Common;

namespace Dynamixel;

public record FailedToOpenPortError(PortNumber PortNumber) 
    : Expected(CreateMessage(PortNumber), Code: 0)
{
    private static string CreateMessage(PortNumber portNumber)
        => $"Failed to open port with: {portNumber}";
}
