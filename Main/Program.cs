using Dynamixel;
using ManualBehavior;
using MaydayDomain;
using MaydayDomain.MotionPlanning;

DynamixelAdapter jointAdapter = new();
DynamixelJointFactory jointFactory = new(jointAdapter);
var structure = MaydayStructure.Create(jointFactory);

InstantPostureMaydayMotionPlanner maydayMotionPlanner = new(structure);
TerminalPostureBehaviorController behaviorController = new(maydayMotionPlanner);

behaviorController.Start();