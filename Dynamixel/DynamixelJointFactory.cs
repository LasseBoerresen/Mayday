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
    
    public static JointFactory CreateWithDynamixelJoints()
    {
        PortAdapter portAdapterSdkImpl = new PortAdapterSdkImpl();
        portAdapterSdkImpl.Initialize();

        Adapter jointAdapter = new AdapterSdkImpl(portAdapterSdkImpl);

        return new DynamixelJointFactory(jointAdapter);
    }

    public void Dispose()
    {
        adapter.Dispose();
        GC.SuppressFinalize(this);
    }
}