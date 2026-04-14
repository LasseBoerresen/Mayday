using Dynamixel;
using LanguageExt;
using MaydayDataAccess;
using MaydayDomain;
using MaydayDomain.MotionPlanning;
using RobotDomain.Behavior;

namespace ManualBehavior;

public class MaydayRobot(BehaviorController behaviorController, CancellationTokenSource cancelTokenSource)
{
    // TODO extract A MaydayRobotFactory, to make it clear that most complexity
    //  is in creating the robot, and that it really only starts and stops.  
    public static Eff<MaydayRobot> CreateWithTerminalPostureBehaviorController(TimeProvider timeProvider) 
    {
        CancellationTokenSource cts = new();
        var jointFactoryEff = DynamixelJointFactory.Create(cts, timeProvider);
        var legPostureByPosition = new LegPostureByPositionMapFileRepo().Load();
        
        return jointFactoryEff
            .Map(jointFactory => new MaydayLegFactory(jointFactory, legPostureByPosition))
            .Map(legFactory => InstantPostureMaydayMotionPlanner.Create(legFactory))
            .Map(motionPlanner => new TerminalPostureBehaviorController(motionPlanner, cts, timeProvider))
            .Map(behaviorController => new MaydayRobot(behaviorController, cts));
    }

    public static Eff<MaydayRobot> CreateWithBabyLegsBehaviorController(TimeProvider timeProvider)
    {
        CancellationTokenSource cts = new();
        var jointFactoryEff = DynamixelJointFactory.Create(cts, timeProvider);
        var legPostureByPosition = new LegPostureByPositionMapFileRepo().Load(); 
        
        return jointFactoryEff
            .Map(jointFactory => new MaydayLegFactory(jointFactory, legPostureByPosition))
            .Map(legFactory => StepByStepLearningInstantPostureMaydayMotionPlanner.Create(legFactory))
            .Map(motionPlanner => new BabyLegsBehaviorController(motionPlanner, cts, timeProvider))
            .Map(behaviorController => new MaydayRobot(behaviorController, cts));
    }

    public static Eff<MaydayRobot> CreateWithSwayBehavior(TimeProvider timeProvider)
    {
        CancellationTokenSource cts = new();
        var jointFactoryEff = DynamixelJointFactory.Create(cts, timeProvider);
        var legPostureByPosition = new LegPostureByPositionMapFileRepo().Load(); 
        
        return jointFactoryEff
            .Map(jointFactory => new MaydayLegFactory(jointFactory, legPostureByPosition))
            .Map(legFactory => InstantPostureMaydayMotionPlanner.Create(legFactory))
            .Map(motionPlanner => new SwayBehaviorController(motionPlanner, cts.Token, timeProvider))
            .Map(behaviorController => new MaydayRobot(behaviorController, cts));
    }

    public Unit Start() => behaviorController.Start();

    public void Stop() => cancelTokenSource.Cancel();
}
