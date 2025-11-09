using LanguageExt;

namespace RobotDomain.Behavior;

public interface BehaviorController : IDisposable
{
    Unit Start();
}
