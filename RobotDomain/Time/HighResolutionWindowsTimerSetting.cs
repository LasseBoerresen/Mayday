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

    static void DisposeOnProcessExit()
    {
        AppDomain.CurrentDomain.ProcessExit += (s, e) => Instance.Dispose();
    }
    
    void Dispose()
    {
        EndHighTimerResolutionSession();
    }
}