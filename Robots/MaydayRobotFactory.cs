using Dynamixel;
using RobotDomain.Time;
using Generic.System;
using LanguageExt;
using ManualBehavior;
using MaydayDataAccess;
using MaydayDomain;
using MaydayDomain.MotionPlanning;
using RobotDomain.Behavior;
using Robots.Base;

namespace Robots;

public class MaydayRobotFactory(LegPostureByPositionMap LegPostureByPositionMap, FatalErrorHandler fatalErrorHandler)
{
    public Eff<MaydayRobot> CreateWithTerminalPostureBehaviorController(Terminal terminal, TimeProvider timeProvider) 
    {
        CancellationTokenSource cts = new();
        var jointFactoryEff = DynamixelJointFactory.Create(cts, timeProvider, fatalErrorHandler);
        
        return jointFactoryEff
            .Map(jointFactory => new MaydayLegFactory(jointFactory, LegPostureByPositionMap))
            .Map(legFactory => new MaydayStructureFactory(legFactory).CreateDefault())
            .Map(structure => new TrackingMaydayMotionPlanner(structure, timeProvider, fatalErrorHandler))
            .Map(motionPlanner => new TerminalPostureBehaviorController(motionPlanner, terminal, cts.Token, timeProvider))
            .Map(behaviorController => new MaydayRobot(behaviorController, cts));
    }

    public Eff<MaydayRobot> CreateWithBabyLegsBehaviorController(TimeProvider timeProvider)
    {
        CancellationTokenSource cts = new();
        var jointFactoryEff = DynamixelJointFactory.Create(cts, timeProvider, fatalErrorHandler);
        
        return jointFactoryEff
            .Map(jointFactory => new MaydayLegFactory(jointFactory, LegPostureByPositionMap))
            .Map(legFactory => new MaydayStructureFactory(legFactory).CreateDefault())
            .Map(structure => new StepByStepLearningMaydayMotionPlanner(
                structure, 
                new InverseLegKinematicsNeuralNetwortTensorflowNetImpl(),
                timeProvider,
                fatalErrorHandler))
            .Map(motionPlanner => new BabyLegsBehaviorController(motionPlanner, cts, timeProvider))
            .Map(behaviorController => new MaydayRobot(behaviorController, cts));
    }

    public Eff<MaydayRobot> CreateWithSwayBehavior(TimeProvider timeProvider)
    {
        CancellationTokenSource cts = new();
        var jointFactoryEff = DynamixelJointFactory.Create(cts, timeProvider, fatalErrorHandler);
        
        return jointFactoryEff
            .Map(jointFactory => new MaydayLegFactory(jointFactory, LegPostureByPositionMap))
            .Map(legFactory => new MaydayStructureFactory(legFactory).CreateDefault())
            .Map(structure => new TrackingMaydayMotionPlanner(structure, timeProvider, fatalErrorHandler))
            .Map(motionPlanner => new SwayBehaviorController(motionPlanner, cts.Token, timeProvider, fatalErrorHandler))
            .Map(behaviorController => new MaydayRobot(behaviorController, cts));
    }
}
