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
        var portAdapterEff = PortAdapterSdkImpl.CreateInitialized();
        
        return portAdapterEff.Map(portAdapter => Create(portAdapter, cancellationTokenSource, timeProvider));
    }

    static DynamixelJointFactory Create(
        PortAdapter portAdapter, 
        CancellationTokenSource cancellationTokenSource, 
        TimeProvider timeProvider)
    {
        JointStateCacheDictImpl jointStateCache = new();

        var jointAdapter = new AdapterSdkImpl(portAdapter, jointStateCache, cancellationTokenSource, timeProvider);

        return new DynamixelJointFactory(jointAdapter);
    }

    public void Dispose()
    {
        adapter.Dispose();
        GC.SuppressFinalize(this);
    }
}