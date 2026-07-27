using LanguageExt;
using RobotDomain.Geometry;
using RobotDomain.Structures;

namespace Dynamixel;

public class DynamixelJointFactory(Adapter adapter) : JointFactory, IDisposable
{
    public Joint New(
        Link parent,
        Link child,
        Transform passiveTransform,
        JointId id,
        RobotDomain.Structures.RotationDirection rotationDirection,
        AttachmentOrder attachmentOrder)
    {
        DynamixelJoint joint = new(id, adapter, passiveTransform, rotationDirection, attachmentOrder, parent, child);

        joint.Initialize();

        return joint;
    }
    
    public static Eff<DynamixelJointFactory> Create(
        CancellationTokenSource cancellationTokenSource, 
        TimeProvider timeProvider)
    {
        var communicationBusEff = CommunicationBusSdkImpl.CreateInitialized();
        
        return communicationBusEff.Map(communicationBus => 
            Create(communicationBus, cancellationTokenSource, timeProvider));
    }

    static DynamixelJointFactory Create(
        CommunicationBus communicationBus, 
        CancellationTokenSource cancellationTokenSource, 
        TimeProvider timeProvider)
    {
        JointStateCacheDictImpl jointStateCache = new();

        var jointAdapter = new AdapterSdkImpl(communicationBus, jointStateCache, cancellationTokenSource, timeProvider);

        return new DynamixelJointFactory(jointAdapter);
    }

    public void Dispose()
    {
        adapter.Dispose();
        GC.SuppressFinalize(this);
    }
}