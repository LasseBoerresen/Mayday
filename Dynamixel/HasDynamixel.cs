using LanguageExt;

namespace Dynamixel;

public interface HasDynamixel<RT> 
    where RT : struct, HasDynamixel<RT>
{
    Eff<RT, DynamixelIO> DynamixelEff { get; }
}

public struct LiveRuntime : HasDynamixel<LiveRuntime>
{
    public Eff<LiveRuntime, DynamixelIO> DynamixelEff
        => LiveDynamixelIOSdkImpl
            .CreateInitialized()
            .Map(DynamixelIO (dio) => dio)
            .WithRuntime<LiveRuntime>();
}

public static class Dynamixel<RT> 
    where RT : struct, HasDynamixel<RT>
{
    public static Eff<RT, uint> Read(Id id, ControlRegister cr) =>
        default(RT).DynamixelEff.Map(rt => rt.Read(id, cr));

    public static Eff<RT, Unit> Write(Id id, ControlRegister cr, uint value) =>
        default(RT).DynamixelEff.Map(rt => rt.Write(id, cr, value));
        
    public static Eff<RT, Unit> Reboot(Id id) =>
        default(RT).DynamixelEff.Map(rt => rt.Reboot(id));
    
    public static Eff<RT, bool> Ping(Id id, ControlRegister cr) =>
        default(RT).DynamixelEff.Map(rt => rt.Ping(id));
}
