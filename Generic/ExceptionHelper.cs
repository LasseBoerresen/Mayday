using static System.Console;

namespace Generic;

public static class ExceptionHelper
{
    public static void ThrowExceptionOnAnyKey(string message)
    {
        WriteLine(message);
        WriteLine("Press any key to terminate...");
        ReadKey();
        throw new(message);
    }
}
