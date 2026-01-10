using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Generic;
using LanguageExt;
using LanguageExt.Common;
using UnitsNet;
using static System.Console;
using static Dynamixel.DynamixelCommunication;
using Error = LanguageExt.Common.Error;
using FTD2XX_NET;
using Duration = UnitsNet.Duration;

namespace Dynamixel;

// TODO this class should be a singleton, since it represents a single port. Also the dynamixel adapter.
public class PortAdapterSdkImpl : PortAdapter
{
    const int CommunicationSuccessCode = 0;
    const int ProtocolVersion = 2;
    readonly PortNumber _portNumber;
    readonly object _lock = new();
    readonly IDictionary<ControlRegister, GroupNumber> _writeGroupsByControlRegister 
        = new Dictionary<ControlRegister, GroupNumber>();
    readonly IDictionary<ControlRegister, GroupNumber> _readGroupsByControlRegister 
        = new Dictionary<ControlRegister, GroupNumber>();
        
        

    PortAdapterSdkImpl(PortNumber portNumber)
    {
        _portNumber = portNumber;
    }

    static readonly BitRate BitRate = BitRate.FromBitsPerSecond(4000000);
    static readonly Duration PacketTimeOut = Duration.FromMilliseconds(2);
    static readonly Duration DeviceLatency = Duration.FromMilliseconds(1);
    
     
    // Check which port is being used on your controller
    // ex) Windows: "COM1"   Linux: "/dev/ttyUSB0" Mac: "/dev/tty.usbserial-*"
    const string FtdiDeviceSerialNumber = "FT2H2YMW";

    public void Write(IReadOnlyDictionary<Id, uint> valuesById, ControlRegister cr)
    {
        if (!valuesById.Any())
            return;
        
        lock (_lock)
        {
            var group = GetWriteGroupFor(cr);

            groupSyncWriteClearParam(group.Value);
            
            valuesById.ForEach(kvp =>
            {
                var dxl_addparam_result = groupSyncWriteAddParam(group.Value, kvp.Key, kvp.Value, cr.SizeInBytes);
                if (dxl_addparam_result != true)
                    throw new Exception($"[ID: {kvp.Key}] groupSyncWrite addparam failed");
            });

            // var sw = Stopwatch.StartNew();
            groupSyncWriteTxPacket(group.Value);
            // sw.Stop();
            // WriteLine($"Write time: {sw.Elapsed}");
            //
            // sw.Restart();
            CheckCommunicationResults(mode: nameof(Write), cr: cr);
            // WriteLine($"check Communication time: {sw.Elapsed}");
        }
    }

    public IReadOnlyDictionary<Id, uint> Read(IEnumerable<Id> ids, ControlRegister cr)
    {
        // TODO should probably check communication results of every command. 
        
        if(!ids.Any())
            return new Dictionary<Id, uint>();
        
        lock (_lock)
        {
            var group = GetReadGroupFor(cr);

            groupSyncReadClearParam(group.Value);
            
            ids.ForEach(id =>
            {
                var dxl_addparam_result = groupSyncReadAddParam(group.Value, id);
                if (dxl_addparam_result != true)
                    throw new Exception($"[ID: {id}] groupSyncRead addparam failed");
            });
            
            groupSyncReadTxRxPacket(group.Value);
            CheckCommunicationResults(mode: nameof(Read), cr: cr);

            ids.ForEach(
                EnsureDataIsAvailableFor);
            
            var result = ids
                .Select(id => (id, groupSyncReadGetData(group.Value, id, cr.Address, cr.SizeInBytes)))
                .ToDictionary();

            return result;

            // Local func
            void EnsureDataIsAvailableFor(Id id)
            {
                if (!groupSyncReadIsAvailable(group.Value, id, cr.Address, cr.SizeInBytes)) 
                    throw new Exception($"No data available for dxl_id {id} and control register {cr}");
            }
        }
    }

    GroupNumber GetWriteGroupFor(ControlRegister cr)
    {
        if (!_writeGroupsByControlRegister.ContainsKey(cr))
        {
            var groupId = groupSyncWrite(_portNumber.Value, ProtocolVersion, cr.Address, cr.SizeInBytes);
            _writeGroupsByControlRegister[cr] = new GroupNumber(groupId);
        }

        var group = _writeGroupsByControlRegister[cr];
        return group;
    }

    GroupNumber GetReadGroupFor(ControlRegister cr)
    {
        if (!_readGroupsByControlRegister.ContainsKey(cr))
        {
            var groupId = groupSyncRead(_portNumber.Value, ProtocolVersion, cr.Address, cr.SizeInBytes);
            _readGroupsByControlRegister[cr] = new GroupNumber(groupId);
        }
        
        var group = _readGroupsByControlRegister[cr];
        return group;
    }

    public void Write(Id id, ControlRegister cr, uint value)
    {
        lock(_lock)
        {
            WriteBySize(id, cr, value);

            CheckCommunicationResults(nameof(Write), id, cr, value);
        }
    }

    public uint Read(Id id, ControlRegister cr)
    {
        lock (_lock)
        {
            var result = ReadBySize(id, cr);

            CheckCommunicationResults(nameof(Read), id, cr);

            return result;
        }
    }

    public void Reboot(Id id)
    {
        lock (_lock)
        {
            try
            {
                reboot(_portNumber.Value, ProtocolVersion, id);
                CheckCommunicationResults(nameof(Reboot), id, Option<ControlRegister>.None);
            }
            catch (Exception e)
            {
                WriteLine("retrying reboot after 1s");
                Thread.Sleep(1000);
                reboot(_portNumber.Value, ProtocolVersion, id);
            }
        }
    }

    public bool Ping(Id id)
    {
        lock (_lock)
        {
            ping(_portNumber.Value, ProtocolVersion, id);

            try
            {
                CheckCommunicationResults(nameof(Ping), id, Option<ControlRegister>.None);
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }
    }

    void WriteBySize(Id id, ControlRegister cr, uint value)
    {
        switch (cr.SizeInBytes)
        {
            case 1:
                write1ByteTxRx(_portNumber.Value, ProtocolVersion, id, cr.Address, (byte)value);
                break;
            case 2:
                write2ByteTxRx(_portNumber.Value, ProtocolVersion, id, cr.Address, (ushort)value);
                break;
            case 4:
                write4ByteTxRx(_portNumber.Value, ProtocolVersion, id, cr.Address, value);
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
                return read1ByteTxRx(_portNumber.Value, ProtocolVersion, id, cr.Address);
            case 2:
                return read2ByteTxRx(_portNumber.Value, ProtocolVersion, id, cr.Address);
            case 4:
                return read4ByteTxRx(_portNumber.Value, ProtocolVersion, id, cr.Address);
            default:
                throw new NotSupportedException($"ControlRegister size not supported, got: {cr}");
        }
    }

    /// <remarks>
    /// Must be used withing the lock of the original communication
    /// </remarks>
    void CheckCommunicationResults(
        string mode,
        Option<Id> id = default,
        Option<ControlRegister> cr = default,
        Option<uint> value = default)
    {
        var crMessage = cr.Map(s => $"and control register {s}");
        var valueMessage = value.Map(s => $"and value {s}");
        var idMessage = id.Map(id => $"dxl_id {id}");
        var errorMessage = $"{mode} {idMessage.IfNone("all ids")} {crMessage.IfNone("")} {valueMessage.IfNone("")} gave error:\n";
        
        var lastTxRxResult = getLastTxRxResult(_portNumber.Value, ProtocolVersion);
        if (lastTxRxResult != CommunicationSuccessCode)
            throw new(errorMessage + Marshal.PtrToStringAnsi(getTxRxResult(ProtocolVersion, lastTxRxResult)));

        var lastRxPacketError = getLastRxPacketError(_portNumber.Value, ProtocolVersion);
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
    
    static Eff<Unit> SetPortPacketTimeOut(PortNumber portNumber)
    {
        setPacketTimeoutMSec(portNumber.Value, (uint)PacketTimeOut.Milliseconds);
        
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
                .Bind(_ => SetPortPacketTimeOut(portNumber))
                .Map(_ => new PortAdapterSdkImpl(portNumber)));
    }

    static Eff<Unit> InitializePacketHandler()
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

    static Eff<PortNumber> InitializePortHandlerAndGetNumber()
    {
        return ConfigurePortLatencyAndGetComPortName()
            .Map(deviceName => new PortNumber(portHandler(deviceName)));
    }

    static Eff<string> ConfigurePortLatencyAndGetComPortName()
    {
        var device = new FTDI();
        
        try
        {
            // Note: Ensure DeviceName matches the FTDI Description string, 
            // not necessarily the COM port string.
            var ftStatus = device.OpenBySerialNumber(FtdiDeviceSerialNumber);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
                return Error.New($"Failed to open device with SN '{FtdiDeviceSerialNumber}', status: {ftStatus}");

            // TODO: BUG, this does not seem to actually change the com port
            //  latency, and it only works after manually changing it for the
            //  com port in windows device manager. 
            ftStatus = device.SetLatency((byte)DeviceLatency.Milliseconds);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
                return Error.New($"Failed to set port latency, status: {ftStatus}");
            
            // OBS! COM ports only work on Windows. If on linux, implement
            // different lookup or use /dev/ttyUSB0 instead
            ftStatus = device.GetCOMPort(out var comPortName);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
                return Error.New($"Failed to get COM port name, status: {ftStatus}");

            return Eff<string>.Pure(comPortName);
        }
        finally
        {
            if (device.IsOpen) 
                device.Close();
        }
    }
}