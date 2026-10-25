# Test project

xUnit with AwesomeAssertions and Moq. Read [README.md](README.md) for what lives where.

## Placement

- `Unit/<Project>/`: one collaborator in isolation, mirroring the production project.
- `Integration/<Project>/`: several real components together.
- `Utilities/`: physical-test attributes, `TestConfiguration`, `TestTerminal`. Object mothers live in each area's `Base/TestObjectMother.cs`; reuse them, and `Unit/Dynamixel/EchoJointDriver.cs` for hardware-free joints, before writing new helpers.

## Naming and shape

- Test names: `Given<State>_When<Action>_Then<Outcome>`, with `// Given`, `// When`, `// Then` sections in the body.
- **Several inputs, one behavior:** a `[Theory]` with `[MemberData]` backed by `TheoryData<TestInput>`, one row per case. `TestInput` is a record nested in the test class with descriptive members for inputs and expected results; build cases with named arguments and a `TestId`. Pattern: `Unit/Components/ThoraxTests.cs`.
- Single simple values may use `[InlineData]`. Use a loop only when iteration itself is the behavior under test.

## Time and determinism

Use `TimeProvider` and `FakeTimeProvider` (`Microsoft.Extensions.TimeProvider.Testing`) to control time. Advance the fake clock instead of sleeping on the wall clock.

## Physical tests

`[PhysicalRobotFact]` / `[PhysicalRobotTheory]` tests drive the real robot. They skip unless `MAYDAY_ROBOT_IS_CLEAR_TO_MOVE` is `True` in the Test project's user secrets or environment. The value is `False` on the development machine; keep it that way unless the user sets it themselves. Mark any new test that needs hardware with one of these attributes: they carry the trait `Tier=Physical`, which keeps them out of the Stop hook's gate. The pre-push gate runs them on the robot, so a code push moves it. Never use `Skip`: the pre-push gate fails on a skipped test outside `Tier=Manual`; quarantine a known failure instead.

`[ManualRobotFact]` tests also need a person at the robot: they ask yes/no questions through `HumanVerifier` and skip unless `MAYDAY_HUMAN_IS_PRESENT` is `True` as well. Use them for physical behavior no sensor can report. Confirm the starting state with the human before the move under test, and phrase each question with what to look at and the expected size of the move. Never set `MAYDAY_HUMAN_IS_PRESENT` yourself.
