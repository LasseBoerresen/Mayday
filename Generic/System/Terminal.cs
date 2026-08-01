namespace Generic.System;

public interface Terminal
{
    void Write(string? value);
    
    void WriteLine(string? value);

    string? ReadLine();
}