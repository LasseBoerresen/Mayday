using AwesomeAssertions;
using Test.Utilities;
using Xunit;

namespace Test.Unit.Utilities;

public class HumanVerifierTests
{
    public record TestInput(string TestId, string[] Answers, bool ExpectedConfirmation, int ExpectedQuestionsAsked);

    public static TheoryData<TestInput> AnswerCases =>
    [
        new(TestId: "y", Answers: ["y"], ExpectedConfirmation: true, ExpectedQuestionsAsked: 1),
        new(TestId: "yes, upper case and padded", Answers: [" YES "], ExpectedConfirmation: true, ExpectedQuestionsAsked: 1),
        new(TestId: "n", Answers: ["n"], ExpectedConfirmation: false, ExpectedQuestionsAsked: 1),
        new(TestId: "no", Answers: ["No"], ExpectedConfirmation: false, ExpectedQuestionsAsked: 1),
        new(TestId: "unclear, then y", Answers: ["maybe", "", "y"], ExpectedConfirmation: true, ExpectedQuestionsAsked: 3),
    ];

    [Theory]
    [MemberData(nameof(AnswerCases))]
    public void GivenAnswers_WhenConfirms_ThenReturnsTheFirstClearAnswer(TestInput input)
    {
        // Given
        ScriptedHumanPrompt prompt = new(input.Answers);
        HumanVerifier verifier = new(prompt);

        // When
        var confirmed = verifier.Confirms("Did it move?");

        // Then
        confirmed.Should().Be(input.ExpectedConfirmation, input.TestId);
        prompt.Questions.Should().HaveCount(input.ExpectedQuestionsAsked, input.TestId);
    }

    [Fact]
    public void GivenQuestion_WhenConfirms_ThenAsksItWithTheAllowedAnswers()
    {
        // Given
        ScriptedHumanPrompt prompt = new(["y"]);
        HumanVerifier verifier = new(prompt);

        // When
        verifier.Confirms("Did it move?");

        // Then
        prompt.Questions.Single().Should().Contain("Did it move?").And.Contain("y/n");
    }

    [Fact]
    public void GivenHumanGivesNoAnswer_WhenConfirms_ThenThrows()
    {
        // Given
        ScriptedHumanPrompt prompt = new([null]);
        HumanVerifier verifier = new(prompt);

        // When
        var confirm = () => verifier.Confirms("Did it move?");

        // Then
        confirm.Should().Throw<HumanVerifier.NoAnswerException>();
    }

    sealed class ScriptedHumanPrompt(IEnumerable<string?> answers) : HumanPrompt
    {
        readonly Queue<string?> _answers = new(answers);

        public List<string> Questions { get; } = [];

        public string? Ask(string question)
        {
            Questions.Add(question);
            return _answers.Dequeue();
        }
    }
}
