namespace RobotDomain.Time;

/// <summary>
/// Decides what happens when the action run by a <see cref="PeriodicScheduler"/> throws.
/// </summary>
/// <remarks>
/// A periodic action drives a control loop, so an exception escaping it is treated as fatal. Production passes
/// <see cref="FailFastFatalErrorHandler"/>, which ends the process; tests pass a handler that records the exception
/// instead, so a mistake in a test cannot take down the test host. Composition roots choose the implementation.
/// </remarks>
public interface FatalErrorHandler
{
    /// <summary>
    /// Reacts to an exception that escaped a periodic action. Called on the scheduler's thread.
    /// </summary>
    /// <param name="exception">The exception thrown by the periodic action.</param>
    /// <remarks>
    /// Implementations used on a real robot must not swallow the error: they end the process and do not return.
    /// </remarks>
    void Handle(Exception exception);
}
