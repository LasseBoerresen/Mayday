# Unit tests

## Overview

Fast, focused tests of one Mayday or shared-layer collaborator at a time
(namespace `Test.Unit`). Each subfolder mirrors a production project or
namespace; the folder itself holds the shared assertion helpers.

## Responsibility

Fast focused tests for Mayday and shared collaborators.

## Public APIs

Tests, fakes, and test object mothers; no production API.

## Interface

### Shared helpers

#### `TestObjectFactory`

Shared tolerances, assertions and a mock joint for unit and integration tests.
Static class. File: `TestObjectFactory.cs`.

| Member | What it does |
|---|---|
| `CreateMockJoint()` | A `Mock<Joint>` attached `LinkFirst` between new `Base` and `Thorax` links, with `Transform.Zero` and `RotationDirection.Forward`. |
| `AssertTransformEqual(testId, expected, actual)` | Asserts `Transform.IsAlmostEqual` within 0.01 m and 0.01 rev. |
| `AssertXyzEqual(testId, expected, actual[, precision])` | Asserts `Xyz.IsAlmostEqual` within `precision`, default 0.01 m. |
| `AssertLengthEqual(testId, expected, actual[, precision])` | Asserts the absolute difference is at most `precision`, default 0.01 m. |
| `TestMessage(testId, expected, actual)` | The failure message the assertions use: test id, expected and actual. |
| `DataFor_GivenLegWithJointsAtZero_WhenGetLinkTransform_ThenReturnsExpected()` | Expected link transforms (`CoxaMotor` to `Tip`) of a leg with all joints at zero, in the leg's own frame (`CoxaMotor` at the origin), as positional `TheoryData<string, LinkName, Transform>`. |
| `TimeProvider` | A fresh `Mock.Of<TimeProvider>()`. No test uses it. |

Used by `Unit.MaydayDomain`, `Unit.Components`, `Unit.RobotDomain.Geometry`
and by `Test.Integration.Main` and `Test.Integration.MaydayDomain`.

#### `TestId`

Names one case of a theory, so a failure says which row failed. Record with
implicit conversions from and to `string`. File: `TestId.cs`. Used by the
`TestInput` records of `ThoraxTests` and `DriverTests`, and by integration
tests.

### Subfolders

| Folder | What it tests |
|---|---|
| [Base](./Base/README.md) | No tests: the marker interface `TestObjectMother`. |
| [Components](./Components/README.md) | `Link`/`Attachment` wiring and `Thorax.ViewedFrom`; the `TheoryData<TestInput>` pattern. |
| [Dynamixel](./Dynamixel/README.md) | `StepAngle`, `DynamixelRotationalSpeed`, `Driver`, `DynamixelJointFactory`, `PeriodicallyBatchedJointDriver`; the `EchoJointDriver` test double. |
| [Generic](./Generic/README.md) | `UnitsNetExtensions.IsAlmostEqualSingleRotation`. |
| [MaydayDataAccess](./MaydayDataAccess/README.md) | `LegPostureByPositionMapFileRepo` with the JSON maps in `TestData`. |
| [MaydayDomain](./MaydayDomain/README.md) | `DefaultMaydayStructure`, `MaydayLeg`, posture maps, `Thorax`, `Stance`. |
| [RobotDomain](./RobotDomain/README.md) | Geometry, motion plans, `PeriodicScheduler`; the `FakeJoint` test double. |
| [Robots](./Robots/README.md) | `MaydayRobotFactory.CreateWithSwayBehavior`. |
| [Utilities](./Utilities/README.md) | `HumanVerifier` from `Test/Utilities`. |

## Key Concepts

Arrange observable behavior with fake joints and controllable inputs.

For cases with several inputs or expected values, follow [ThoraxTests](Components/ThoraxTests.cs): `[MemberData]` provides `TheoryData<TestInput>`, with a test-local record and descriptive member names. Prefer this to positional `[InlineData]` when it improves readability and type safety; retain `[InlineData]` for simple scalar cases.

## Dependencies

Production projects through `Test.csproj`; xUnit, Moq, and AwesomeAssertions.

## Dependency Rules

Do not move test-specific helpers into production projects for convenience.

## Invariants

Keep ordinary unit tests independent of real devices. No unit test carries
`Tier=Physical`, and none drives a joint. One exception touches hardware:
`Robots/Mayday`'s test builds `MaydayRobotFactory.CreateWithSwayBehavior`,
which opens the FTDI adapter (serial number `FT2H2YMW`) to set its latency
when the adapter is plugged in, without moving the robot.

The only `Tier=Manual` test, `MaydayDataAccess`'s map rebuilder, is also
skipped; it keeps the trait so the pre-push gate does not fail on the skip.
No unit test is quarantined.

The default tolerances of `TestObjectFactory` are 1 cm and 0.01 rev
(3.6°). A test that needs finer checks passes its own `precision`, as
`Stance` tests do with 0.1 mm.

## Side Effects

- `PeriodicScheduler`, used by `RobotDomain/Time` and `Dynamixel` tests, sets
  the test process to high priority and 1 ms Windows timer resolution for the
  rest of the run, and runs a busy-waiting loop thread.
- `MaydayDataAccess` tests read their JSON maps from the output directory.
- `Robots/Mayday` opens and closes the FTDI adapter when it is plugged in.
- Otherwise none: no serial port, user secrets or windows.

## Verification

- Run: `dotnet test Test\Test.csproj --filter "FullyQualifiedName~Test.Unit&Quarantine!=true"`.
  It moves no robot; it does reach the FTDI adapter (see Invariants).

## Architectural Constraints

Use existing helpers and `TimeProvider` when timing affects tests.

## Common Pitfalls

[Physical runs](../../knowledge/pitfalls/physical-runs.md).

## Related ADRs

[Hardware boundary](../../docs/adr/0002-hardware-adapter-boundary.md).

## Related Findings

None recorded.

## Related Root Causes

[Interpolation drift](../../knowledge/root-causes/interpolation-baseline-drift.md).

## Related Lessons Learned

[Goal versus measured state](../../knowledge/lessons-learned/goal-versus-measured-state.md).

## Related Failed Attempts

[Measured-angle interpolation](../../knowledge/failed-attempts/measured-angle-interpolation.md).
