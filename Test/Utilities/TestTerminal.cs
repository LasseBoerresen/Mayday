using Generic.System;

namespace Test.Utilities;

public class TestTerminal(Queue<string> inputLines) : Terminal
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
        if (inputLines.Count == 0)
            return null;
            
        var line = inputLines.Dequeue();
        Console.WriteLine(line);
        return line;
    }
    
    public void InputLine(string line)
    {
        inputLines.Enqueue(line);
    }
}
