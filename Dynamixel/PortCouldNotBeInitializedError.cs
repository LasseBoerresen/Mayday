using LanguageExt.Common;

namespace Dynamixel;

public record PortCouldNotBeInitializedError(PortName PortName) 
    : Expected(Message: CreateMessage(PortName), Code: 0) 
{
    private static string CreateMessage(PortName portName) 
        => $"Could not initialize port handler for device '{portName}'";
}
