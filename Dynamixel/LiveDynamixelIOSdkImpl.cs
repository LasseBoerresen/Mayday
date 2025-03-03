using System.Runtime.InteropServices;
using LanguageExt;
using UnitsNet;
using static System.Console;
using static Dynamixel.Sdk;
using static Generic.EffUnitExtensions;
using static Generic.ExceptionHelper;
using Error = LanguageExt.Common.Error;

namespace Dynamixel;

public class LiveDynamixelIOSdkImpl: DynamixelIO
{
    private LiveDynamixelIOSdkImpl(PortNumber portNumber)
    {
        _portNumber = portNumber;
    }

    const int CommunicationSuccessCode = 0;
    const int ProtocolVersion = 2;
    readonly PortNumber _portNumber;
    readonly object _lock = new();

    static readonly BitRate BitRate = BitRate.FromBitsPerSecond(4000000); 
     
    // Check which port is being used on your controller
    // ex) Windows: "COM1"   Linux: "/dev/ttyUSB0" Mac: "/dev/tty.usbserial-*"
    static readonly PortName PortName = new("COM9");

    public static Eff<LiveDynamixelIOSdkImpl> CreateInitialized()
    {
        var portNumber = InitializePortHandlerAndGetNumber();
        
        return portNumber.Map(pn => new LiveDynamixelIOSdkImpl(pn));
    }

    static Eff<PortNumber> InitializePortHandlerAndGetNumber()
    {
        PortNumber portNumber = new(portHandler(PortName.Value));

        if (portNumber.Value == 0)
            return new PortCouldNotBeInitializedError(PortName);

        InitializePacketHandler();
        OpenPort(portNumber);
        SetPortBaudrate(portNumber);

        return Eff<PortNumber>.Pure(portNumber);
    }

    static Eff<Unit> InitializePacketHandler()
    {
        try
        {
            packetHandler();
        }
        catch (Exception e)
        {
            return Error.New($"Error initializing packet handler: {e.Message}");
        }
        
        return Eff<Unit>.Pure(Unit.Default);
    }

    public Unit Write(Id id, ControlRegister cr, uint value)
    {
        lock(_lock)
            WriteBySize(id, cr, value);
        
        CheckCommunicationResults(id, cr, "writing", value);
        
        return Unit.Default;
    }

    public uint Read(Id id, ControlRegister cr)
    {
        uint result;
    
        lock(_lock)
            result = ReadBySize(id, cr);
        
        CheckCommunicationResults(id, cr, "reading");

        return result;
    }

    public Unit Reboot(Id id)
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

        return Unit.Default;
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
            return new FailedToOpenPortError(portNumber);
        
        return UnitEff;
    }

    static Eff<Unit> SetPortBaudrate(PortNumber portNumber)
    {
        var wasSuccess = setBaudRate(portNumber.Value, (int)BitRate.BitsPerSecond);
        if (!wasSuccess)
            return Error.New($"Failed to set the port handler baudrate to: '{BitRate}'");
        
        return UnitEff;
    }

    public void Dispose()
    {
        closePort(_portNumber.Value);
        GC.SuppressFinalize(this);
    }
}