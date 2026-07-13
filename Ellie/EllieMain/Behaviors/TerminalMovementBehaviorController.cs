using LanguageExt;
using RobotDomain.Behavior;

namespace EllieMain.Behaviors;

public class TerminalMovementBehaviorController(CancellationToken ct) : BehaviorController
{
    public Unit Start()
    {
        PrintCommandList();
        while (!ct.IsCancellationRequested)
            ExecuteConsoleCommand();
            
        return Unit.Default;
    }

    void PrintCommandList()
    {
        throw new NotImplementedException();
    }

    void ExecuteConsoleCommand()
    {
        throw new NotImplementedException();
    }
}
