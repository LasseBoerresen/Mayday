using System.Diagnostics;

namespace Test.Utilities;

/// <summary>Shows a question to a human and returns the line they type.</summary>
public interface HumanPrompt
{
    /// <summary>Returns the human's answer to <paramref name="question"/>, or null when they gave none.</summary>
    string? Ask(string question);
}

/// <summary>
/// Asks in a new console window, because the test host has no interactive console: <c>dotnet test</c> and IDE
/// runners redirect its standard input. Windows only.
/// </summary>
public sealed class ConsoleWindowHumanPrompt(TimeSpan timeout) : HumanPrompt
{
    /// <summary>
    /// Opens a PowerShell window showing <paramref name="question"/> and waits for the typed answer.
    /// Returns null when the window is closed without answering.
    /// </summary>
    /// <exception cref="PlatformNotSupportedException">Not running on Windows.</exception>
    /// <exception cref="TimeoutException">No answer within the timeout; the window is closed.</exception>
    public string? Ask(string question)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException($"{nameof(ConsoleWindowHumanPrompt)} needs Windows.");

        var directory = Directory.CreateTempSubdirectory("mayday-human-prompt-");
        try
        {
            // The question and answer go through files, so no quoting of the question on a command line is needed.
            var questionPath = Path.Combine(directory.FullName, "question.txt");
            var answerPath = Path.Combine(directory.FullName, "answer.txt");
            var scriptPath = Path.Combine(directory.FullName, "ask.ps1");
            File.WriteAllText(questionPath, question);
            File.WriteAllText(scriptPath, AskScript);

            using var window = Process.Start(new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\" "
                    + $"-QuestionPath \"{questionPath}\" -AnswerPath \"{answerPath}\"",
                UseShellExecute = true,
            }) ?? throw new InvalidOperationException("Could not open the prompt window.");

            if (!window.WaitForExit(timeout))
            {
                window.Kill();
                // Kill only requests termination; wait so the window releases its files before the directory goes.
                window.WaitForExit();
                throw new TimeoutException($"No answer within {timeout} to: {question}");
            }

            return File.Exists(answerPath) ? File.ReadAllText(answerPath) : null;
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    const string AskScript = """
        param([string] $QuestionPath, [string] $AnswerPath)
        $host.UI.RawUI.WindowTitle = 'Mayday manual test'
        Write-Host (Get-Content -Raw -LiteralPath $QuestionPath)
        $answer = Read-Host 'Answer'
        Set-Content -LiteralPath $AnswerPath -Value $answer -NoNewline
        """;
}
