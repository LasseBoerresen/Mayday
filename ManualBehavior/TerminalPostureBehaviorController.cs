using LanguageExt;
using MaydayDomain;
using MaydayDomain.MotionPlanning;
using RobotDomain.Behavior;
using RobotDomain.Structures;

namespace ManualBehavior;

public class TerminalPostureBehaviorController(
    MaydayMotionPlanner motionPlanner,
    CancellationTokenSource cancelTokenSource) : BehaviorController
{
    public void Start()
    {
        WakeUp();

        PrintCommandList();
        while (!cancelTokenSource.Token.IsCancellationRequested)
            ExecuteConsoleCommand();
    }

    void ExecuteConsoleCommand()
    {
        var command = GetCommand();
        ExecuteCommand(command);
        Thread.Sleep(TimeSpan.FromSeconds(0.5));
        // Console.WriteLine(motionPlanner.GetPosture());
        Console.WriteLine("TibiaMotor positions");
        Console.WriteLine(motionPlanner.GetPositionsOf(LinkName.TibiaMotor));
        
        // Console.WriteLine("Coxa Orientations");
        // Console.WriteLine(motionPlanner.GetOrientationsOf(LinkName.Femur));
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
            .Some(motionPlanner.SetPosture)
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
        
        motionPlanner.SetPosture(MaydayLegPosture.Sitting);
        Thread.Sleep(TimeSpan.FromSeconds(0.5));
        motionPlanner.SetPosture(MaydayLegPosture.SittingTall);
        Thread.Sleep(TimeSpan.FromSeconds(0.5));
        motionPlanner.SetPosture(MaydayLegPosture.Sitting);
    }

    void Sleep()
    {
        Console.WriteLine("Going to sleep...");
        
        Thread.Sleep(TimeSpan.FromSeconds(0.5));
        motionPlanner.SetPosture(MaydayLegPosture.Sitting);
    }

    void Stop()
    {
        Sleep();
        Thread.Sleep(TimeSpan.FromSeconds(2));
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
