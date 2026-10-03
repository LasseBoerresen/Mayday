using LanguageExt;
using RobotDomain.Time;
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
        TimeProvider timeProvider,
        FatalErrorHandler fatalErrorHandler)
    {
        var communicationBusEff = NativeSerialPortCommunicationBus.CreateInitialized();
        
        return communicationBusEff.Map(communicationBus => 
            Create(communicationBus, cancellationTokenSource, timeProvider, fatalErrorHandler));
    }

    static DynamixelJointFactory Create(
        CommunicationBus communicationBus, 
        CancellationTokenSource cancellationTokenSource, 
        TimeProvider timeProvider,
        FatalErrorHandler fatalErrorHandler)
    {
        JointStateCacheDictImpl jointStateCache = new();

        Driver driver = new(communicationBus);

        var jointDriver = new PeriodicallyBatchedJointDriver(
            driver, 
            jointStateCache, 
            cancellationTokenSource, 
            timeProvider,
            fatalErrorHandler);

        return new DynamixelJointFactory(jointDriver);
    }

    public void Dispose()
    {
        jointDriver.Dispose();
        GC.SuppressFinalize(this);
    }
}