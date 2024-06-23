using Dynamixel;
using ManualBehavior;
using MaydayDomain;
using MaydayDomain.MotionPlanning;
using RobotDomain.Behavior;
using RobotDomain.Structures;

namespace Main;

public class MaydayRobot(BehaviorController behaviorController)
{
    public static MaydayRobot Create()
    {
        JointFactory jointFactory = CreateJointFactory();

        var structure = MaydayStructure.Create(jointFactory);

        MaydayMotionPlanner maydayMotionPlanner = new InstantPostureMaydayMotionPlanner(structure);
        BehaviorController behaviorController = new TerminalPostureBehaviorController(maydayMotionPlanner);

        return new(behaviorController);

    }

    public void Start()
    {
        behaviorController.Start();
    }

    static JointFactory CreateJointFactory()
    {
        PortAdapter portAdapterSdkImpl = new PortAdapterSdkImpl();
        
        Adapter jointAdapter = new AdapterSdkImpl(portAdapterSdkImpl);
        
        return new DynamixelJointFactory(jointAdapter);
    }
}
