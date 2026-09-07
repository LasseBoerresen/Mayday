using Dynamixel;
using Generic.System;
using LanguageExt;
using ManualBehavior;
using MaydayDataAccess;
using MaydayDomain;
using MaydayDomain.MotionPlanning;
using RobotDomain.Behavior;
using Robots.Base;

namespace Robots;

public class MaydayRobotFactory(LegPostureByPositionMap LegPostureByPositionMap)
{
    public Eff<MaydayRobot> CreateWithTerminalPostureBehaviorController(Terminal terminal, TimeProvider timeProvider) 
    {
        CancellationTokenSource cts = new();
        var jointFactoryEff = DynamixelJointFactory.Create(cts, timeProvider);
        
        return jointFactoryEff
            .Map(jointFactory => new MaydayLegFactory(jointFactory, LegPostureByPositionMap))
            .Map(legFactory => new MaydayStructureFactory(legFactory).CreateDefault())
            .Map(structure => new InstantPostureMaydayMotionPlanner(structure, timeProvider))
            .Map(motionPlanner => new TerminalPostureBehaviorController(motionPlanner, terminal, cts.Token, timeProvider))
            .Map(behaviorController => new MaydayRobot(behaviorController, cts));
    }

    public Eff<MaydayRobot> CreateWithBabyLegsBehaviorController(TimeProvider timeProvider)
    {
        CancellationTokenSource cts = new();
        var jointFactoryEff = DynamixelJointFactory.Create(cts, timeProvider);
        
        return jointFactoryEff
            .Map(jointFactory => new MaydayLegFactory(jointFactory, LegPostureByPositionMap))
            .Map(legFactory => new MaydayStructureFactory(legFactory).CreateDefault())
            .Map(structure => new StepByStepLearningInstantPostureMaydayMotionPlanner(
                structure, 
                new InverseLegKinematicsNeuralNetwortTensorflowNetImpl(),
                timeProvider))
            .Map(motionPlanner => new BabyLegsBehaviorController(motionPlanner, cts, timeProvider))
            .Map(behaviorController => new MaydayRobot(behaviorController, cts));
    }

    public Eff<MaydayRobot> CreateWithSwayBehavior(TimeProvider timeProvider)
    {
        CancellationTokenSource cts = new();
        var jointFactoryEff = DynamixelJointFactory.Create(cts, timeProvider);
        
        return jointFactoryEff
            .Map(jointFactory => new MaydayLegFactory(jointFactory, LegPostureByPositionMap))
            .Map(legFactory => new MaydayStructureFactory(legFactory).CreateDefault())
            .Map(structure => new InstantPostureMaydayMotionPlanner(structure, timeProvider))
            .Map(motionPlanner => new SwayBehaviorController(motionPlanner, cts.Token, timeProvider))
            .Map(behaviorController => new MaydayRobot(behaviorController, cts));
    }
}
