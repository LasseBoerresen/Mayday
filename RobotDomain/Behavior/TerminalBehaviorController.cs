using Generic.System;
using LanguageExt;

namespace RobotDomain.Behavior;

public abstract class TerminalBehaviorController<TCommand>(Terminal terminal, CancellationToken cancelToken)
    : BehaviorController where TCommand : struct, Enum
{
    

    public Unit Start()
    {
        WakeUp();

        PrintCommandList();
        while (!cancelToken.IsCancellationRequested)
        {
            var command = GetCommand();
            if (command == null) // terminal has no more inputs. 
                break;
            
            ExecuteCommand(command.Value);
        
            PrintUpdate();
        }

        return Unit.Default;
    }

    protected void WakeUp()
    {
        terminal.WriteLine("\nWaking up...");
        
        WakeUpBehavior();
    }

    protected abstract void WakeUpBehavior();

    protected void Sleep()
    {
        terminal.WriteLine("Going to sleep...");
        
        SleepBehavior();
    }

    protected abstract void SleepBehavior();


    protected abstract void ExecuteCommand(TCommand command);

    TCommand? GetCommand()
    {
        terminal.Write("Next command: ");
        var inputLine = terminal.ReadLine();
        if (inputLine == null) 
            return null;

        var commandStringWasValid = Enum.TryParse<TCommand>(inputLine, ignoreCase: true, out var command);
        if (!commandStringWasValid)
        {
            terminal.WriteLine($"Invalid command: '{inputLine}'");
            return GetCommand();
        }

        return command;
    }

    protected abstract void PrintUpdate();
    
    void PrintCommandList()
    {
        var commandListString = Enum
            .GetValues<TCommand>()
            .Aggregate("\n", (s, pc) => s + $"{Convert.ToInt32(pc)}: {pc}\n");
     
        terminal.Write(commandListString);
    }
}