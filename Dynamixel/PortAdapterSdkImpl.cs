using System.Runtime.InteropServices;
using LanguageExt;
using UnitsNet;
using static System.Console;
using static Dynamixel.Sdk;
using static Generic.ExceptionHelper;

namespace Dynamixel;

public class PortAdapterSdkImpl: PortAdapter
{
    const int CommunicationSuccessCode = 0;
    const int ProtocolVersion = 2;
    int _portNumber;
    readonly object _lock = new();

    static readonly BitRate BitRate = BitRate.FromBitsPerSecond(4000000); 
     
    // Check which port is being used on your controller
    // ex) Windows: "COM1"   Linux: "/dev/ttyUSB0" Mac: "/dev/tty.usbserial-*"
    const string DeviceName = "COM9";

    public void Initialize()
    {
        _portNumber = portHandler(DeviceName);
        packetHandler();
        
        OpenPort();
        SetPortBaudrate();
    }

    public void Write(Id id, ControlRegister cr, uint value)
    {
        lock(_lock)
            WriteBySize(id, cr, value);
        
        CheckCommunicationResults(id, cr, "writing", value);
    }

    public uint Read(Id id, ControlRegister cr)
    {
        uint result;
    
        lock(_lock)
            result = ReadBySize(id, cr);
        
        CheckCommunicationResults(id, cr, "reading");

        return result;
    }

    public void Reboot(Id id)
    {
        try
        {
            lock(_lock)
                reboot(_portNumber, ProtocolVersion, (byte)id.Value);
            CheckCommunicationResults(id, Option<ControlRegister>.None, "reboot");
        }
        catch (Exception e)
        {
            WriteLine("retrying reboot after 1s");
            Thread.Sleep(1000);
            lock(_lock)
                reboot(_portNumber, ProtocolVersion, (byte)id.Value);
        }
        
    }

    public bool Ping(Id id)
    {
        lock(_lock)
            ping(_portNumber, ProtocolVersion, (byte)id.Value);
            
        try
        {
            CheckCommunicationResults(id, Option<ControlRegister>.None, "ping");
        }
        catch (Exception e)
        {
            return false;
        }

        return true;
    }

    void WriteBySize(Id id, ControlRegister cr, uint value)
    {
        switch (cr.SizeInBytes)
        {
            case 1:
                write1ByteTxRx(_portNumber, ProtocolVersion, (byte)id.Value, cr.Address, (byte)value);
                break;
            case 2:
                write2ByteTxRx(_portNumber, ProtocolVersion, (byte)id.Value, cr.Address, (ushort)value);
                break;
            case 4:
                write4ByteTxRx(_portNumber, ProtocolVersion, (byte)id.Value, cr.Address, value);
                break;
            default:
                throw new NotSupportedException($"ControlRegister size not supported, got: {cr}");
        }
    }

    uint ReadBySize(Id id, ControlRegister cr)
    {
        switch (cr.SizeInBytes)
        {
            case 1:
                return read1ByteTxRx(_portNumber, ProtocolVersion, (byte)id.Value, cr.Address);
            case 2:
                return read2ByteTxRx(_portNumber, ProtocolVersion, (byte)id.Value, cr.Address);
            case 4:
                return read4ByteTxRx(_portNumber, ProtocolVersion, (byte)id.Value, cr.Address);
            default:
                throw new NotSupportedException($"ControlRegister size not supported, got: {cr}");
        }
    }

    void CheckCommunicationResults(Id id, Option<ControlRegister> cr, string mode, Option<uint> value=default)
    {
        var crMessage = cr.Map(s => $"and control register {s}");
        var valueMessage = value.Map(s => $"and value {s}");
        var errorMessage = $"{mode} dxl_id {id} {crMessage.IfNone("")} {valueMessage.IfNone("")} gave error:\n";


        int lastTxRxResult;
        lock(_lock)
            lastTxRxResult = getLastTxRxResult(_portNumber, ProtocolVersion);
        if (lastTxRxResult != CommunicationSuccessCode)
            throw new(errorMessage + Marshal.PtrToStringAnsi(getTxRxResult(ProtocolVersion, lastTxRxResult)));

        byte lastRxPacketError;
        lock(_lock) 
            lastRxPacketError = getLastRxPacketError(_portNumber, ProtocolVersion);
        if (lastRxPacketError != CommunicationSuccessCode)
            throw new(errorMessage + Marshal.PtrToStringAnsi(getRxPacketError(ProtocolVersion, lastRxPacketError)));
    }
    
    void OpenPort()
    {
        var wasSuccess = openPort(_portNumber);
        if (wasSuccess)
            WriteLine($"Succeeded to open the port with number: '{_portNumber}'!");
        else
            ThrowExceptionOnAnyKey($"Failed to open port with number: '{_portNumber}'");
    }

    void SetPortBaudrate()
    {
        var wasSuccess = setBaudRate(_portNumber, (int)BitRate.BitsPerSecond);
        if (wasSuccess)
            WriteLine($"Succeeded to set the port handler baudrate to '{BitRate}'!");
        else
            ThrowExceptionOnAnyKey($"Failed to set the port handler baudrate to: '{BitRate}'");
    }

    public void Dispose()
    {
        closePort(_portNumber);
        GC.SuppressFinalize(this);
    }
}
