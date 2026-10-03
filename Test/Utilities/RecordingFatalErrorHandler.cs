using System.Collections.Concurrent;
using RobotDomain.Time;

namespace Test.Utilities;

/// <summary>
/// A <see cref="FatalErrorHandler"/> for tests. It records the exception, where production would end the process, so
/// a mistake in a periodic loop shows up as a failed assertion and cannot take down the test host.
/// </summary>
/// <remarks>
/// Do not use it for physical tests: they drive a real robot and keep <see cref="FailFastFatalErrorHandler"/>.
/// </remarks>
public sealed class RecordingFatalErrorHandler : FatalErrorHandler
{
    readonly ConcurrentQueue<Exception> _exceptions = new();

    /// <summary>The exceptions handed to <see cref="Handle"/>, oldest first. Safe to read from any thread.</summary>
    public IReadOnlyCollection<Exception> Exceptions => _exceptions;

    public void Handle(Exception exception) => _exceptions.Enqueue(exception);
}
