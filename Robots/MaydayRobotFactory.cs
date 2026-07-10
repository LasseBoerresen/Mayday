using Dynamixel;
using LanguageExt;
using ManualBehavior;
using MaydayDataAccess;
using MaydayDomain;
using MaydayDomain.MotionPlanning;
using RobotDomain.Behavior;

namespace Robots;

public static class MaydayRobotFactory
{
    public static Eff<MaydayRobot> CreateWithTerminalPostureBehaviorController(TimeProvider timeProvider) 
    {
        CancellationTokenSource cts = new();
        var jointFactoryEff = DynamixelJointFactory.Create(cts, timeProvider);
        var legPostureByPositionMap = new LegPostureByPositionMapFileRepo().Load();
        
        return jointFactoryEff
            .Map(jointFactory => new MaydayLegFactory(jointFactory, legPostureByPositionMap))
            .Map(legFactory => new MaydayStructureFactory(legFactory).CreateDefault())
            .Map(structure => new InstantPostureMaydayMotionPlanner(structure))
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
            .Map(legFactory => new MaydayStructureFactory(legFactory).CreateDefault())
            .Map(structure => new StepByStepLearningInstantPostureMaydayMotionPlanner(
                structure, new InverseLegKinematicsNeuralNetwortTensorflowNetImpl()))
            .Map(motionPlanner => new BabyLegsBehaviorController(motionPlanner, cts, timeProvider))
            .Map(behaviorController => new MaydayRobot(behaviorController, cts));
    }

    public static Eff<MaydayRobot> CreateWithSwayBehavior(TimeProvider timeProvider)
    {
        CancellationTokenSource cts = new();
        var jointFactoryEff = DynamixelJointFactory.Create(cts, timeProvider);
        var legPostureByPositionMap = new LegPostureByPositionMapFileRepo().Load(); // should probably also return Eff 
        
        return jointFactoryEff
            .Map(jointFactory => new MaydayLegFactory(jointFactory, legPostureByPositionMap))
            .Map(legFactory => new MaydayStructureFactory(legFactory).CreateDefault())
            .Map(structure => new InstantPostureMaydayMotionPlanner(structure))
            .Map(motionPlanner => new SwayBehaviorController(motionPlanner, cts.Token, timeProvider))
            .Map(behaviorController => new MaydayRobot(behaviorController, cts));
    }
}
