using RobotDomain.Geometry;
using RobotDomain.Structures;
using UnitsNet;

namespace Dynamixel;

public class DynamixelJoint : Joint
{
    readonly Transform _passiveTransform;
    readonly JointId _id;
    readonly Side _side;
    readonly Adapter _adapter;

    public DynamixelJoint(
        Link parent, 
        Link child, 
        Transform passiveTransform, 
        JointId id, 
        Side side, 
        Adapter adapter) 
        : base(ComponentId.New, parent, child)
    {
        _passiveTransform = passiveTransform;
        _id = id;
        _side = side;
        _adapter = adapter;
    }
    
    public override JointState State => _adapter.GetState(_id); 
    
    public override void SetAngleGoal(Angle goal) => _adapter.SetGoal(_id, goal);

    public void Initialize() => _adapter.Initialize(_id);

    protected override Transform Transform => ActiveTransform + _passiveTransform;

    Transform ActiveTransform => Transform.FromQ(Q.FromRpy(new(Angle.Zero, Angle.Zero, Angle)));

    Angle Angle => State.Angle * RotationDirectionMultiplier;

    double RotationDirectionMultiplier => _side == Side.Left ? 1 : -1;
}
