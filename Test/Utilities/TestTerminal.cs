using Generic.System;

namespace Test.Utilities;

public class TestTerminal(Stack<string> inputLines) : Terminal
{
    public void Write(string? value)
    {
        Console.Write(value);
    }

    public void WriteLine(string? value)
    {
        Console.WriteLine(value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public string? ReadLine()
    {
        return inputLines.Count != 0 
            ?  inputLines.Pop()
            : null;
    }
}
