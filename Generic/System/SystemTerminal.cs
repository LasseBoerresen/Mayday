namespace Generic.System;

public class SystemTerminal : Terminal
{
    public void Write(string? value) => Console.Write(value);

    public void WriteLine(string? value) => Console.WriteLine(value);

    public string? ReadLine() => Console.ReadLine();
}
