using LanguageExt;
using RobotDomain.Geometry;
using RobotDomain.Motion;
using RobotDomain.Structures;

namespace Dynamixel;

public class DynamixelJointFactory(JointDriver jointDriver) : JointFactory, IDisposable
{
    public Joint New(
        Link parent,
        Link child,
        Transform passiveTransform,
        JointId id,
        RobotDomain.Structures.RotationDirection rotationDirection,
        AttachmentOrder attachmentOrder)
    {
        DynamixelJoint joint = new(id, jointDriver, passiveTransform, rotationDirection, attachmentOrder, parent, child);

        joint.Initialize();

        return joint;
    }
    
    public static Eff<DynamixelJointFactory> Create(
        CancellationTokenSource cancellationTokenSource, 
        TimeProvider timeProvider)
    {
        var communicationBusEff = NativeSerialPortCommunicationBus.CreateInitialized();
        
        return communicationBusEff.Map(communicationBus => 
            Create(communicationBus, cancellationTokenSource, timeProvider));
    }

    static DynamixelJointFactory Create(
        CommunicationBus communicationBus, 
        CancellationTokenSource cancellationTokenSource, 
        TimeProvider timeProvider)
    {
        JointStateCacheDictImpl jointStateCache = new();

        Driver()

        var jointDriver = new PeriodicallyScheduledJointDriver(
            communicationBus, 
            jointStateCache, 
            cancellationTokenSource, 
            timeProvider);

        return new DynamixelJointFactory(jointDriver);
    }

    public void Dispose()
    {
        jointDriver.Dispose();
        GC.SuppressFinalize(this);
    }
}