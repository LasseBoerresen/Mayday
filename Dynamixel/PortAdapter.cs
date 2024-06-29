namespace Dynamixel;

public interface PortAdapter : IDisposable
{
    public void Initialize();
    public void Write(Id id, ControlRegister cr, uint value);
    public uint Read(Id id, ControlRegister cr);
    public void Reboot(Id id);
    public bool Ping(Id id);
}
