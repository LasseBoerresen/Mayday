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

`[PhysicalRobotFact]` / `[PhysicalRobotTheory]` tests drive the real robot. They skip unless `MAYDAY_ROBOT_IS_CONNECTED` is `True` in the Test project's user secrets or environment. The value is `False` on the development machine; keep it that way unless the user sets it themselves. Mark any new test that needs hardware with one of these attributes.
