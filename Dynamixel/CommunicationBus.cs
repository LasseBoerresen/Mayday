namespace Dynamixel;

/// <summary>
/// Allows basic communication with dynamixels such as write, read, reboot etc.  
/// </summary>
/// <remarks>
/// Can target a single or multiple dynamixels at a time for efficiency. 
/// </remarks>
public interface CommunicationBus : IDisposable
{
    public void Write(IReadOnlyDictionary<Id, uint> valuesById, ControlRegister cr);
    public IReadOnlyDictionary<Id, uint> Read(IEnumerable<Id> ids, ControlRegister cr);
    public void Write(Id id, ControlRegister cr, uint value);
    public uint Read(Id id, ControlRegister cr);
    public void Reboot(Id id);
    public bool Ping(Id id);
}
