using System.Runtime.InteropServices;
using UnitsNet;

namespace RobotDomain.Time;

public class HighResolutionWindowsTimerSetting
{
    public static HighResolutionWindowsTimerSetting Instance { get; } = new();

    HighResolutionWindowsTimerSetting()
    {
        StartHighTimerResolutionSession();

        DisposeOnProcessExit();
    }

    static readonly Duration HighTimerResolution = Duration.FromMilliseconds(1);
    static void StartHighTimerResolutionSession() => TimeBeginPeriod((uint)HighTimerResolution.Milliseconds);
    static void EndHighTimerResolutionSession() => TimeEndPeriod((uint)HighTimerResolution.Milliseconds);
    
    [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
    static extern uint TimeBeginPeriod(uint uMilliseconds);

    [DllImport("winmm.dll", EntryPoint = "timeEndPeriod")]
    static extern uint TimeEndPeriod(uint uMilliseconds);

    [DllImport("ntdll.dll", SetLastError = true)]
    static extern int NtQueryTimerResolution(out uint minimumResolution, out uint maximumResolution, out uint currentResolution);
    
    static uint GetCurrentResolution()
    {
        NtQueryTimerResolution(out _, out _, out uint currentResolution);
        return currentResolution; // Resolution is in 100-nanosecond units
    }
    
    public void VerifyTimerResolution()
    {
        var current = GetCurrentResolution();
        
        // 1ms = 10,000 units of 100ns
        Console.WriteLine($"Current Timer Resolution: {current / 10000.0} ms");
    }
    
    static void DisposeOnProcessExit()
    {
        AppDomain.CurrentDomain.ProcessExit += (s, e) => Instance.Dispose();
    }
    
    void Dispose()
    {
        EndHighTimerResolutionSession();
    }
}