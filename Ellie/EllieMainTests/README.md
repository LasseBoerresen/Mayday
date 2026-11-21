# EllieMainTests

## Overview

The xUnit project (`EllieMainTests.csproj`, AwesomeAssertions, Moq) for Ellie.
Its end-to-end tests start a whole Ellie from
[EllieMain](../EllieMain/README.md) with a mocked `ActuatorDriver`, so they
touch no hardware. It is not part of the gate (`scripts\gate.ps1`).

## Responsibility

Ellie end-to-end tests in `EndToEnd`.

## Public APIs

`EllieTests` and test extensions; not a production API.

## Interface

- [EndToEnd](./EndToEnd/README.md): `EllieTests` (creating Ellie, wheel
  initialization on start, forward acceleration reaching the wheels) and
  `EllieTestExtensions.StartWaitActWaitStopWait`.

No source files at the project root.

## Key Concepts

Exercises Ellie composition and motion behavior: the real
`TerminalMovementBehaviorController`, `ArticulatedSteeringEllieMotionPlanner`,
`DefaultEllieStructure` and `PeriodicallyBatchedWheelDriver`, driven through a
`TestTerminal`, with Moq standing in for the `ActuatorDriver`.

## Dependencies

`EllieMain`, `Dynamixel`, and the shared `Test` project. The tests use
`Test.Utilities` (`TestTerminal`, `RecordingFatalErrorHandler`); no test uses
`Dynamixel` directly.

## Dependency Rules

Keep tests outside Ellie production code; inspect hardware usage before running.

## Invariants

Unimplemented Ellie structure/planning methods remain unimplemented; tests must
not imply otherwise. Two tests do: they expect `ActuatorDriver.Initialize` and
`RotateAt` calls that the current code never makes, and fail. ADR 0004 records
these two failures; they are not quarantined. Details in
[EndToEnd](./EndToEnd/README.md#invariants).

No test here uses `PhysicalRobotFact`, `PhysicalRobotTheory` or
`ManualRobotFact`, and none constructs a serial bus, `Driver` or
`DynamixelJointFactory`: the current tests initialize no physical device. A
test that did would not be gated by `Tier`, so mark it with one of those
attributes.

## Architectural Constraints

Avoid running tests that initialize physical devices without explicit confirmation.

## Side Effects

- Starting Ellie creates a `PeriodicScheduler`, which raises the test process
  to `ProcessPriorityClass.High`, requests a 1 ms Windows timer resolution and
  busy-waits on a thread-pool thread until `Stop`.
- Console output from `TestTerminal` and the behavior controller.

## Verification

- Run: `dotnet test Ellie\EllieMainTests\EllieMainTests.csproj`. All tests
  are hardware-free; two are expected to fail (above).

## Common Pitfalls

[Physical runs](../../knowledge/pitfalls/physical-runs.md).

## Related ADRs

[Hardware boundary](../../docs/adr/0002-hardware-adapter-boundary.md),
[quarantine](../../docs/adr/0004-quarantine-failing-tests-with-a-trait.md).

## Related Findings

[Scheduler constructed inside domain classes](../../knowledge/findings/scheduler-constructed-inside-domain-classes.md)
(why tests pass a recording fatal-error handler).

## Related Root Causes

None recorded.

## Related Lessons Learned

None recorded.

## Related Failed Attempts

None recorded.
