using LanguageExt;
using MaydayDomain;
using MaydayDomain.MotionPlanning;
using RobotDomain.Behavior;
using RobotDomain.Structures;
using RobotDomain.Time;

namespace ManualBehavior;

public class TerminalPostureBehaviorController(
    MaydayMotionPlanner motionPlanner,
    CancellationToken cancelToken,
    TimeProvider timeProvider) 
    : TerminalBehaviorController<PostureCommand>(cancelToken)
{
    static readonly TimeSpan TimeStep = TimeSpan.FromSeconds(1.0);

    protected override void PrintUpdate()
    {
        Console.WriteLine("Tip positions");
        Console.WriteLine(motionPlanner.GetPositionsOf(LinkName.Tip));
        Console.WriteLine(motionPlanner.GetOrientationsOf(LinkName.Tip));
    }

    protected override void ExecuteCommand(PostureCommand command)
    {
        LookForLegPosture(command)
            .Some(SchedulePosture)
            .None(() => DoComplexCommand(command));
    }

    static Option<MaydayLegPosture> LookForLegPosture(PostureCommand command)
    {
        return command switch
        {
            PostureCommand.Stand => MaydayLegPosture.Standing,
            PostureCommand.StandHigh => MaydayLegPosture.StandingHigh,
            PostureCommand.StandWide => MaydayLegPosture.StandingWide,
            PostureCommand.Sit => MaydayLegPosture.Sitting,
            PostureCommand.SitTall => MaydayLegPosture.SittingTall,
            PostureCommand.Neutral => MaydayLegPosture.Neutral,
            PostureCommand.NeutralWithBackTwist => MaydayLegPosture.NeutralWithBackTwist,
            PostureCommand.Straight => MaydayLegPosture.Straight,
            PostureCommand.StraightWithBackTwist => MaydayLegPosture.StraightWithBackTwist,
            PostureCommand.NeutralWithStraightFemur => MaydayLegPosture.NeutralWithStraightFemur,
            _ => Option<MaydayLegPosture>.None
        };
    }

    void DoComplexCommand(PostureCommand command)
    {
        switch (command)
        {
            case PostureCommand.Sleep:
                Sleep();
                break;
            case PostureCommand.Wake:
                WakeUp();
                break;
        }
    }

    protected override void WakeUpBehavior()
    {
        SchedulePosture(MaydayLegPosture.Sitting);
        SchedulePosture(MaydayLegPosture.SittingTall);
        SchedulePosture(MaydayLegPosture.Sitting);
    }

    protected override void SleepBehavior()
    {
        SchedulePosture(MaydayLegPosture.Sitting);
    }
    
    void SchedulePosture(MaydayLegPosture posture)
    {
        motionPlanner.SetPosture(timeProvider.ScheduleIn(posture, TimeStep));
        Thread.Sleep(TimeStep);
    }
}