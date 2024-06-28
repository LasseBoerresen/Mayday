using Dynamixel;
using ManualBehavior;
using MaydayDomain;
using MaydayDomain.MotionPlanning;
using RobotDomain.Behavior;
using RobotDomain.Structures;

namespace Main;

public class MaydayRobot(BehaviorController behaviorController, CancellationTokenSource cancelTokenSource)
{
    public static MaydayRobot CreateWithTerminalPostureBehaviorController()
    {
        JointFactory jointFactory = CreateJointFactory();

        var structure = MaydayStructure.Create(jointFactory);

        MaydayMotionPlanner maydayMotionPlanner = new InstantPostureMaydayMotionPlanner(structure);

        CancellationTokenSource cts = new();
        BehaviorController behaviorController = new TerminalPostureBehaviorController(maydayMotionPlanner, cts);

        return new(behaviorController, cts);
    }

    public void Start() => behaviorController.Start();

    public void Stop() => cancelTokenSource.Cancel();

    static JointFactory CreateJointFactory()
    {
        PortAdapter portAdapterSdkImpl = new PortAdapterSdkImpl();
        portAdapterSdkImpl.Initialize();

        Adapter jointAdapter = new AdapterSdkImpl(portAdapterSdkImpl);

        return new DynamixelJointFactory(jointAdapter);
    }
}
