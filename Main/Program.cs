using Dynamixel;
using ManualBehavior;
using MaydayDomain;
using MaydayDomain.MotionPlanning;
using RobotDomain.Behavior;
using RobotDomain.Structures;

PortAdapter portAdapterSdkImpl = new PortAdapterSdkImpl();
Adapter jointAdapter = new AdapterSdkImpl(portAdapterSdkImpl);
JointFactory jointFactory = new DynamixelJointFactory(jointAdapter);

var structure = MaydayStructure.Create(jointFactory);

MaydayMotionPlanner maydayMotionPlanner = new InstantPostureMaydayMotionPlanner(structure);
BehaviorController behaviorController = new TerminalPostureBehaviorController(maydayMotionPlanner);

behaviorController.Start();