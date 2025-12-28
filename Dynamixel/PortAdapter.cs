namespace Dynamixel;

public interface PortAdapter : IDisposable
{
    public void Write(IReadOnlyDictionary<Id, uint> valuesById, ControlRegister cr);
    public IReadOnlyDictionary<Id, uint> Read(IEnumerable<Id> ids, ControlRegister cr);
    public void Write(Id id, ControlRegister cr, uint value);
    public uint Read(Id id, ControlRegister cr);
    public void Reboot(Id id);
    public bool Ping(Id id);
}
