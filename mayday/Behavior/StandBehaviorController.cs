using System;
using mayday.mayday;

namespace mayday.Behavior;

public class StandBehaviorController(MaydayStructure structure) : BehaviorController
{
    public void Start()
    {
        while (true)
            ExecuteConsoleCommand();
    }

    public void Stop() => Environment.Exit(0);
    public void Stand() => structure.Posture = MaydayPosture.Standing;

    public void Sit() => structure.Posture = MaydayPosture.Sitting;

    private void ExecuteConsoleCommand()
    {
        var command = GetCommand();
        ExecuteCommand(command);
    }

    private static string GetCommand()
    {
        Console.Write("Enter command (stand/sit/stop): ");
        return Console.ReadLine()?.ToLower() ?? "";
    }

    private void ExecuteCommand(string command)
    {
        if (command.ToLower() == "stand")
            Stand();
        else if (command.ToLower() == "sit")
            Sit();
        else if (command.ToLower() == "stop")
            Stop();
        else
            Console.WriteLine($"Invalid command '{command}'. Please enter either 'stand', 'sit' or 'stop'.");
    }

}
