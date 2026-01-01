using LanguageExt;
using MaydayDomain;
using MaydayDomain.MotionPlanning;
using RobotDomain.Behavior;
using RobotDomain.Structures;
using RobotDomain.Time;

namespace ManualBehavior;

public class TerminalPostureBehaviorController(
    MaydayMotionPlanner motionPlanner,
    CancellationTokenSource cancelTokenSource,
    TimeProvider timeProvider) 
    : BehaviorController
{
    static readonly TimeSpan TimeStep = TimeSpan.FromSeconds(1.0);
    
    public Unit Start()
    {
        WakeUp();

        PrintCommandList();
        while (!cancelTokenSource.Token.IsCancellationRequested)
            ExecuteConsoleCommand();
            
        return Unit.Default;
    }

    void ExecuteConsoleCommand()
    {
        var command = GetCommand();
        ExecuteCommand(command);
        
        Console.WriteLine("Tip positions");
        Console.WriteLine(motionPlanner.GetPositionsOf(LinkName.Tip));
        Console.WriteLine(motionPlanner.GetOrientationsOf(LinkName.Tip));
    }

    static PostureCommand GetCommand()
    {
        Console.Write("Next command: ");
        var commandString = Console.ReadLine()?.ToLower() ?? "";
        if (!Enum.TryParse<PostureCommand>(commandString, ignoreCase: true, out var command))
        {
            Console.WriteLine($"Invalid command: '{commandString}'");
            return GetCommand();
        }

        return command;
    }

    void ExecuteCommand(PostureCommand command)
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
            case PostureCommand.Stop:
                Stop();
                break;
            case PostureCommand.Sleep:
                Sleep();
                break;
            case PostureCommand.Wake:
                WakeUp();
                break;
        }
    }

    void WakeUp()
    {
        Console.WriteLine("\nWaking up...");
        
        SchedulePosture(MaydayLegPosture.Sitting);
        SchedulePosture(MaydayLegPosture.SittingTall);
        SchedulePosture(MaydayLegPosture.Sitting);
    }

    void Sleep()
    {
        Console.WriteLine("Going to sleep...");
        
        SchedulePosture(MaydayLegPosture.Sitting);
    }
    
    void SchedulePosture(MaydayLegPosture posture)
    {
        motionPlanner.SetPosture(timeProvider.ScheduleIn(posture, TimeStep));
        Thread.Sleep(TimeStep);
    }

    void Stop()
    {
        Sleep();
        
        cancelTokenSource.Cancel();
    }

    static void PrintCommandList()
    {
        var commandListString = Enum
            .GetValues<PostureCommand>()
            .Aggregate("\n", (s, pc) => s + $"{(int)pc}: {pc}\n");
     
        Console.Write(commandListString);
    }
}
