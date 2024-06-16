using ManualBehavior;
using MaydayDomain;
using RobotDomain;
using RobotDomain.MotionPlanning;

var maydayStructure = MaydayStructure.Create();
InstantPostureMotionPlanner motionPlanner = new();
TerminalPostureBehaviorController behaviorController = new(motionPlanner);
Mayday may = new(behaviorController);

may.Start();