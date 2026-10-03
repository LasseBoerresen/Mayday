namespace RobotDomain.Time;

/// <summary>
/// Logs the exception and ends the process with <see cref="Environment.FailFast(string, Exception)"/>, so a failing
/// control loop never keeps running unnoticed. This is the handler for composition roots that drive a real robot.
/// </summary>
public sealed class FailFastFatalErrorHandler : FatalErrorHandler
{
    /// <summary>
    /// Writes the exception to the console, then ends the process. Does not return.
    /// </summary>
    public void Handle(Exception exception)
    {
        Console.WriteLine($"Error in periodic task: {exception.Message}, {exception.StackTrace}");

        Environment.FailFast("Unobserved task exception", exception);
    }
}
