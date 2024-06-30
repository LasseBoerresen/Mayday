using MaydayDomain;
using MaydayDomain.MotionPlanning;
using RobotDomain.Behavior;

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

    public void WakeUp()
    {
        Console.WriteLine("\nWaking up...");
        
        Sit();
        Thread.Sleep(TimeSpan.FromSeconds(0.5));
        SitTall();
        Thread.Sleep(TimeSpan.FromSeconds(0.5));
        Sit();
    }
    
    public void Sleep()
    {
        Console.WriteLine("Going to sleep...");
        Thread.Sleep(TimeSpan.FromSeconds(0.5));
        Sit();
        Thread.Sleep(TimeSpan.FromSeconds(2));
    }

    public void Stand()
    {
        motionPlanner.SetPosture(MaydayStructurePosture.Standing);
    }

    public void StandHigh()
    {
        motionPlanner.SetPosture(MaydayStructurePosture.StandingHigh);
    }

    public void StandWide()
    {
        motionPlanner.SetPosture(MaydayStructurePosture.StandingWide);
    }

    public void Sit()
    {
        motionPlanner.SetPosture(MaydayStructurePosture.Sitting);
    }
    
    public void SitTall()
    {
        motionPlanner.SetPosture(MaydayStructurePosture.SittingTall);
    }

    public void Stop()
    {
        Sleep();
        cancelTokenSource.Cancel();
    }

    void ExecuteConsoleCommand()
    {
        var command = GetCommand();
        ExecuteCommand(command);
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
        switch (command)
        {
            case PostureCommand.Stand:
                Stand();
                break;
            case PostureCommand.StandHigh:
                StandHigh();
                break;
            case PostureCommand.StandWide:
                StandWide();
                break; 
            case PostureCommand.Sit:
                Sit();
                break;
            case PostureCommand.SitTall:
                SitTall();
                break;
            case PostureCommand.Stop:
                Stop();
                break;
            case PostureCommand.Sleep:
                Sleep();
                break;
            case PostureCommand.Wake:
                WakeUp();
                break; 
            default:
                throw new NotSupportedException($"PostureCommand {command} was not implemented yet");
        }
    }

    static void PrintCommandList()
    {
        Console.Write(
            "\n" +
            "Commands:\n" +
            "WakeUp(1)\n" +
            "Sleep(2)\n" +
            "Stand(3)\n" +
            "StandWide(4)\n" +
            "StandHigh(5)\n" +
            "Sit(6)\n" +
            "SitTall(7)\n" +
            "Stop(8)\n " +
            "\n");
    }
}
