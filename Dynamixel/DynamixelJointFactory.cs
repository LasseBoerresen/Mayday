using LanguageExt;
using RobotDomain.Geometry;
using RobotDomain.Structures;

namespace Dynamixel;

public class DynamixelJointFactory(Adapter adapter) : JointFactory, IDisposable
{
    public Joint New(
        Link parent,
        Link child,
        Transform transform,
        JointId id,
        RobotDomain.Structures.RotationDirection rotationDirection,
        AttachmentOrder attachmentOrder)
    {
        DynamixelJoint joint = new(
            parent, 
            child, 
            transform, 
            id, 
            rotationDirection, 
            attachmentOrder, 
            adapter);

        joint.Initialize();

        return joint;
    }
    
    public static Eff<DynamixelJointFactory> Create(CancellationTokenSource cancellationTokenSource)
    {
        var portAdapterEff = PortAdapterSdkImpl.CreateInitialized();
        
        return portAdapterEff.Map(portAdapter => Create(portAdapter, cancellationTokenSource));
    }

    static DynamixelJointFactory Create(PortAdapter portAdapter, CancellationTokenSource cancellationTokenSource)
    {
        JointStateCacheDictImpl jointStateCache = new();

        Adapter jointAdapter = new AdapterSdkImpl(portAdapter, jointStateCache, cancellationTokenSource);

        return new DynamixelJointFactory(jointAdapter);
    }

    public void Dispose()
    {
        adapter.Dispose();
        GC.SuppressFinalize(this);
    }
}