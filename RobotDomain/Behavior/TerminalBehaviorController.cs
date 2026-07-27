using LanguageExt;

namespace RobotDomain.Behavior;

public abstract class TerminalBehaviorController<TCommand>(CancellationToken cancelToken)
    : BehaviorController where TCommand : struct, Enum
{
    public Unit Start()
    {
        WakeUp();

        PrintCommandList();
        while (!cancelToken.IsCancellationRequested)
            ExecuteConsoleCommand();
            
        return Unit.Default;
    }

    protected void WakeUp()
    {
        Console.WriteLine("\nWaking up...");
        
        WakeUpBehavior();
    }

    protected abstract void WakeUpBehavior();

    protected void Sleep()
    {
        Console.WriteLine("Going to sleep...");
        
        SleepBehavior();
    }

    protected abstract void SleepBehavior();


    void ExecuteConsoleCommand() 
    {
        var command = GetCommand();
        ExecuteCommand(command);
        
        PrintUpdate();
    }

    protected abstract void ExecuteCommand(TCommand command);

    static TCommand GetCommand()
    {
        Console.Write("Next command: ");
        var commandString = Console.ReadLine()?.ToLower() ?? "";
        if (!Enum.TryParse<TCommand>(commandString, ignoreCase: true, out var command))
        {
            Console.WriteLine($"Invalid command: '{commandString}'");
            return GetCommand();
        }

        return command;
    }

    protected abstract void PrintUpdate();
    
    static void PrintCommandList()
    {
        var commandListString = Enum
            .GetValues<TCommand>()
            .Aggregate("\n", (s, pc) => s + $"{Convert.ToInt32(pc)}: {pc}\n");
     
        Console.Write(commandListString);
    }
}