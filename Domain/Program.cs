using Domain.Behavior;
using Domain.MotionPlanning;
using Domain.Structures;

var maydayStructure = MaydayStructure.Create();
InstantPostureMotionPlanner motionPlanner = new();
TerminalPostureBehaviorController behaviorController = new(motionPlanner);
Domain.Mayday may = new(behaviorController);

may.Start();