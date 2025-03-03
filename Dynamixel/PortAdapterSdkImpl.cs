using System.Runtime.InteropServices;
using LanguageExt;
using LanguageExt.Common;
using UnitsNet;
using static System.Console;
using static Dynamixel.Sdk;
using Error = LanguageExt.Common.Error;

namespace Dynamixel;

public class PortAdapterSdkImpl : PortAdapter
{
    const int CommunicationSuccessCode = 0;
    const int ProtocolVersion = 2;
    readonly PortNumber _portNumber;
    readonly object _lock = new();

    private PortAdapterSdkImpl(PortNumber portNumber)
    {
        _portNumber = portNumber;
    }

    static readonly BitRate BitRate = BitRate.FromBitsPerSecond(4000000); 
     
    // Check which port is being used on your controller
    // ex) Windows: "COM1"   Linux: "/dev/ttyUSB0" Mac: "/dev/tty.usbserial-*"
    const string DeviceName = "COM9";

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
                reboot(_portNumber.Value, ProtocolVersion, (byte)id.Value);
            CheckCommunicationResults(id, Option<ControlRegister>.None, "reboot");
        }
        catch (Exception e)
        {
            WriteLine("retrying reboot after 1s");
            Thread.Sleep(1000);
            lock(_lock)
                reboot(_portNumber.Value, ProtocolVersion, (byte)id.Value);
        }
        
    }

    public bool Ping(Id id)
    {
        lock(_lock)
            ping(_portNumber.Value, ProtocolVersion, (byte)id.Value);
            
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
                write1ByteTxRx(_portNumber.Value, ProtocolVersion, (byte)id.Value, cr.Address, (byte)value);
                break;
            case 2:
                write2ByteTxRx(_portNumber.Value, ProtocolVersion, (byte)id.Value, cr.Address, (ushort)value);
                break;
            case 4:
                write4ByteTxRx(_portNumber.Value, ProtocolVersion, (byte)id.Value, cr.Address, value);
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
                return read1ByteTxRx(_portNumber.Value, ProtocolVersion, (byte)id.Value, cr.Address);
            case 2:
                return read2ByteTxRx(_portNumber.Value, ProtocolVersion, (byte)id.Value, cr.Address);
            case 4:
                return read4ByteTxRx(_portNumber.Value, ProtocolVersion, (byte)id.Value, cr.Address);
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
            lastTxRxResult = getLastTxRxResult(_portNumber.Value, ProtocolVersion);
        if (lastTxRxResult != CommunicationSuccessCode)
            throw new(errorMessage + Marshal.PtrToStringAnsi(getTxRxResult(ProtocolVersion, lastTxRxResult)));

        byte lastRxPacketError;
        lock(_lock) 
            lastRxPacketError = getLastRxPacketError(_portNumber.Value, ProtocolVersion);
        if (lastRxPacketError != CommunicationSuccessCode)
            throw new(errorMessage + Marshal.PtrToStringAnsi(getRxPacketError(ProtocolVersion, lastRxPacketError)));
    }
    
    static Eff<Unit> OpenPort(PortNumber portNumber)
    {
        var wasSuccess = openPort(portNumber.Value);
        if (!wasSuccess)
            return new FailedToOpenPortException(portNumber);
        
        WriteLine($"Succeeded to open the port with: '{portNumber}'!");
        
        return Eff<Unit>.Pure(Unit.Default);
    }

    static Eff<Unit> SetPortBaudrate(PortNumber portNumber)
    {
        var wasSuccess = setBaudRate(portNumber.Value, (int)BitRate.BitsPerSecond);
        if (!wasSuccess)
            return Error.New($"Failed to set the port handler baudrate to: '{BitRate}'");
         
        WriteLine($"Succeeded to set the port handler baudrate to '{BitRate}'!");
        
        return Eff<Unit>.Pure(Unit.Default);
    }

    public void Dispose()
    {
        closePort(_portNumber.Value);
        GC.SuppressFinalize(this);
    }

    public static Eff<PortAdapterSdkImpl> CreateInitialized()
    {
        return InitializePortHandlerAndGetNumber()
            .Bind(portNumber => InitializePacketHandler()
                .Bind(_ => OpenPort(portNumber))
                .Bind(_ => SetPortBaudrate(portNumber))
                .Map(_ => new PortAdapterSdkImpl(portNumber)));
    }

    private static Eff<Unit> InitializePacketHandler()
    {
        try
        {
            packetHandler();
        }
        catch (Exception e)
        {
            return Error.New($"Failed to initialize packet handler: {e.Message}");
        }
        
        return Eff<Unit>.Pure(Unit.Default);
    }

    private static Eff<PortNumber> InitializePortHandlerAndGetNumber()
    {
        var portNumber = new PortNumber(portHandler(DeviceName));
        if(!portNumber.PortIsOpen)
            return Error.New($"Failed to initialize port handler for device '{DeviceName}'");
        
        return Eff<PortNumber>.Pure(portNumber);
    }
}