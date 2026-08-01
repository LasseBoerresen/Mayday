using EllieMain.Base;

namespace EllieMainTests.EndToEnd;

public static class EllieTestExtensions
{
    extension(Ellie ellie)
    {
        /// <summary>
        /// Runs ellie in the background for a moment, then stops it.
        /// </summary>
        /// <remarks>
        /// Anything thrown inside Start is rethrown here, so it fails the test
        /// instead of vanishing into an unobserved task.
        /// </remarks>
        public async Task StartWaitStop(TimeSpan waitTime)
        {
            var startTask = Task.Run(ellie.Start);

            await Task.WhenAny(startTask, Task.Delay(waitTime));   // fails fast if Start throws early
            ellie.Stop();
            await Task.WhenAny(startTask, Task.Delay(waitTime));   // catches failures during shutdown

            if (startTask.IsCompleted)
                await startTask;                                  // rethrows, unwrapped
        }
    }
}
