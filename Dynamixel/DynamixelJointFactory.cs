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
    
    public static Eff<CancellationTokenSource, JointFactory> CreateWithDynamixelJoints(
        CancellationTokenSource cancellationTokenSource)
    {
        var jointFactoryEff = DynamixelIoSdkImpl
            .CreateInitialized()
            .WithRuntime<CancellationTokenSource>()
            .Map(portAdapter => CreateWithDynamixelJoints(portAdapter, cancellationTokenSource));
        
        return jointFactoryEff;
    }

    private static JointFactory CreateWithDynamixelJoints(
        DynamixelIO dynamixelIo, CancellationTokenSource cancellationTokenSource)
    {
        JointStateCacheDictImpl jointStateCache = new();
        AdapterSdkImpl jointAdapter = new(dynamixelIo, jointStateCache, cancellationTokenSource);

        return new DynamixelJointFactory(jointAdapter);
    }

    public void Dispose()
    {
        adapter.Dispose();
        GC.SuppressFinalize(this);
    }
}