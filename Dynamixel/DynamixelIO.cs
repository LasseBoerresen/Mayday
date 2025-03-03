using LanguageExt;

namespace Dynamixel;

public interface DynamixelIO : IDisposable
{
    public Unit Write(Id id, ControlRegister cr, uint value);
    public uint Read(Id id, ControlRegister cr);
    public Unit Reboot(Id id);
    public bool Ping(Id id);
}