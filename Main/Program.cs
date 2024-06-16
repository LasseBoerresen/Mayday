using Dynamixel;
using ManualBehavior;
using MaydayDomain;
using MaydayDomain.MotionPlanning;
using RobotDomain.Behavior;

DynamixelAdapter jointAdapter = new();
DynamixelJointFactory jointFactory = new(jointAdapter);
var structure = MaydayStructure.Create(jointFactory);

MaydayMotionPlanner maydayMotionPlanner = new InstantPostureMaydayMotionPlanner(structure);
BehaviorController behaviorController = new TerminalPostureBehaviorController(maydayMotionPlanner);

behaviorController.Start();