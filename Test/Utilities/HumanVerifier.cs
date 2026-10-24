namespace Test.Utilities;

/// <summary>
/// Asks the human watching the robot a yes/no question, for manual tests whose outcome only a person can see.
/// </summary>
public sealed class HumanVerifier(HumanPrompt prompt)
{
    /// <summary>Thrown when the human closes the prompt without answering.</summary>
    public sealed class NoAnswerException(string question)
        : Exception($"No answer was given to: {question}");

    /// <summary>
    /// Asks <paramref name="question"/> until the human answers yes (<c>y</c>/<c>yes</c>) or no (<c>n</c>/<c>no</c>),
    /// ignoring case and surrounding spaces, and returns whether the answer was yes.
    /// </summary>
    /// <exception cref="NoAnswerException">The human gave no answer.</exception>
    public bool Confirms(string question)
    {
        var prefix = "";

        while (true)
        {
            var answer = prompt.Ask($"{prefix}{question} (y/n)")
                ?? throw new NoAnswerException(question);

            switch (answer.Trim().ToLowerInvariant())
            {
                case "y" or "yes": return true;
                case "n" or "no": return false;
                default: prefix = "Please answer y or n. "; break;
            }
        }
    }
}
