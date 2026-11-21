# Unit tests of the test utilities

## Overview

Unit tests of the shared test infrastructure in
[Test/Utilities](../../Utilities/README.md) (namespace `Test.Unit.Utilities`).
Today they cover only `HumanVerifier`, through a scripted prompt instead of a
real window.

## Interface

### `HumanVerifierTests`

Checks how `HumanVerifier.Confirms` turns typed answers into yes or no.
File: `HumanVerifierTests.cs`.

| Test | What it checks |
|---|---|
| `GivenAnswers_WhenConfirms_ThenReturnsTheFirstClearAnswer` | Theory over `AnswerCases`: `y` and ` YES ` confirm, `n` and `No` deny, each after one question; `maybe`, then an empty answer, then `y` confirms after three questions. |
| `GivenQuestion_WhenConfirms_ThenAsksItWithTheAllowedAnswers` | The single question asked contains the caller's question and `y/n`. |
| `GivenHumanGivesNoAnswer_WhenConfirms_ThenThrows` | A null answer throws `HumanVerifier.NoAnswerException`. |

`ScriptedHumanPrompt`, nested in the test class, is the `HumanPrompt` double:
it records each question in `Questions` and returns the next scripted answer.

## Invariants

- No test here opens `ConsoleWindowHumanPrompt` or touches the robot.
- `AnswerCases` follows the `TheoryData<TestInput>` pattern with a test id
  per row, which the assertions pass as their reason. The id is a `string`
  member named `TestId`, not the `Test.Unit.TestId` record.

## Side Effects

- None.

## Verification

- Run: `dotnet test Test\Test.csproj --filter "FullyQualifiedName~Test.Unit.Utilities"`.
  No `Tier=Physical` or `Tier=Manual` tests are included.
