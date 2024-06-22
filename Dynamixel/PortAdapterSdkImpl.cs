using System.Runtime.InteropServices;
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
        WriteBySize(id, cr, value);
        
        CheckCommunicationResults(id, cr, "writing");
    }

    public uint Read(Id id, ControlRegister cr)
    {
        var result = ReadBySize(id, cr);
        
        CheckCommunicationResults(id, cr, "reading");

        return result;
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

    void CheckCommunicationResults(Id id, ControlRegister cr, string mode)
    {
        var errorMessage = $"{mode} dxl_id {id} and control register {cr} gave error:\n";

        var lastTxRxResult = getLastTxRxResult(_portNumber, ProtocolVersion);
        if (lastTxRxResult != CommunicationSuccessCode)
            throw new(errorMessage + Marshal.PtrToStringAnsi(getTxRxResult(ProtocolVersion, lastTxRxResult)));

        var lastRxPacketError = getLastRxPacketError(_portNumber, ProtocolVersion);
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
    }
}
