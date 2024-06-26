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
        while (!cancelTokenSource.Token.IsCancellationRequested)
            ExecuteConsoleCommand();
    }

    public void Stand() => motionPlanner.SetPosture(MaydayStructurePosture.Standing);

    public void Sit() => motionPlanner.SetPosture(MaydayStructurePosture.Sitting);

    public void Stop() => cancelTokenSource.Cancel();

    void ExecuteConsoleCommand()
    {
        var command = GetCommand();
        ExecuteCommand(command);
    }

    static PostureCommand GetCommand()
    {
        Console.Write("Enter command (stand/sit/stop): ");
        var commandString = Console.ReadLine()?.ToLower() ?? "";
        if (!Enum.TryParse<PostureCommand>(commandString, ignoreCase: true, out var command))
        {
            Console.WriteLine($"Invalid command '{commandString}'. Please enter either 'stand', 'sit' or 'stop'.");
            return GetCommand();
        }

        return command;
    }

    void ExecuteCommand(PostureCommand command)
    {
        if (command == PostureCommand.Stand)
            Stand();
        else if (command == PostureCommand.Sit)
            Sit();
        else if (command == PostureCommand.Stop)
            Stop();
        else
            throw new NotSupportedException($"PostureCommand {command} was not implemented yet");
    }
}
