using MaydayDomain;
using RobotDomain.Behavior;
using RobotDomain.MotionPlanning;

namespace ManualBehavior;

public class TerminalPostureBehaviorController(MotionPlanner motionPlanner) : BehaviorController
{
    public void Start()
    {
        while (true)
            ExecuteConsoleCommand();
    }

    public void Stop() => Environment.Exit(0);
    public void Stand() => motionPlanner.SetPosture(MaydayPosture.Standing);

    public void Sit() => motionPlanner.SetPosture(MaydayPosture.Sitting);

    private void ExecuteConsoleCommand()
    {
        var command = GetCommand();
        ExecuteCommand(command);
    }

    private static PostureCommand GetCommand()
    {
        Console.Write("Enter command (stand/sit/stop): ");
        var commandString = Console.ReadLine()?.ToLower() ?? "";
        if (!Enum.TryParse<PostureCommand>(commandString, ignoreCase: true, out var command))
            Console.WriteLine($"Invalid command '{commandString}'. Please enter either 'stand', 'sit' or 'stop'.");
            GetCommand();
            
        return command;
    }

    private void ExecuteCommand(PostureCommand command)
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