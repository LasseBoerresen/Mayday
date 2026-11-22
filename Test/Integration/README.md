# Integration tests

## Overview

Tests that run several real Mayday components together: the Dynamixel bus and
servos, legs and structures with the real posture map, and whole robots from
the `Robots` factories. Hardware-free tests here run on echo joints; the rest
open the serial adapter and move, or read, the real robot.

## Responsibility

Exercise Mayday component interactions across robot, domain, and adapter boundaries.

## Public APIs

Tests and test object mothers; no production API.

## Interface

No source files of its own. One subdirectory per production project, by the
placement rule in [Test/CLAUDE.md](../CLAUDE.md):

| Subdirectory | What it tests | Tiers |
|---|---|---|
| [Dynamixel](./Dynamixel/README.md) | `NativeSerialPortCommunicationBus` goal-position writes on all 18 servos; servo Drive Mode direction bit against the Mayday model. | `Tier=Physical` only |
| [Main](./Main/README.md) | `MaydayLegTests` (link transforms, quarantined; tip sweep) and `MaydayRobotTests` (start and stop of two robots), on the real robot. | `Tier=Physical`, `Tier=Manual` |
| [MaydayDomain](./MaydayDomain/README.md) | Lean, tip moves and body-frame link transforms of an echo `DefaultMaydayStructure`, echo-leg tip placement, and the manual tips-forward test on the robot. | hardware-free, `Tier=Manual` |
| [MaydayDomain/Base](./MaydayDomain/Base/README.md) | `TestObjectMother`: neutral and default posture maps, echo structures and legs. | no tests |
| [Robots](./Robots/README.md), [Robots/Base](./Robots/Base/README.md) | `TestObjectMother.MaydayRobotFactory`, the real hardware factory. | no tests |

## Key Concepts

Mixed hardware-independent and explicitly marked physical and manual tests.
Hardware-free tests use `EchoJointFactory` joints and the robot's
`DefaultLegPostureByPositionMap.json`; physical tests use
`DynamixelJointFactory` or `NativeSerialPortCommunicationBus` directly.

## Dependencies

`Test.csproj` references Mayday runtime and collaborators.

## Dependency Rules

Keep integration fixtures test-local and check attributes before execution.

## Invariants

`PhysicalRobotFact`/`PhysicalRobotTheory` (trait `Tier=Physical`) mark tests
that need the robot, and `ManualRobotFact` (trait `Tier=Manual`) tests that
also need a person at it. They skip unless `MAYDAY_ROBOT_IS_CLEAR_TO_MOVE` is
`True`, and for manual tests `MAYDAY_HUMAN_IS_PRESENT` as well, in the Test
user secrets or the environment (`TestConfiguration`; user secrets win). They
do not check that the robot is connected: without the adapter they fail
opening it.

Physical tests here do not release the serial adapter, except
`RotationDirectionOnRobotTests` (which closes its bus) and the manual test in
`MaydayDomain` (which disposes its joint factory but not the bus). Only one
owner can open the adapter, so run each physical method in its own process, as
`gate.ps1 -Physical` does
([MAY-81: Need: Mayday physical tests release the robot's port, so they all pass in one test process](https://linear.app/mayday-robot/issue/MAY-81)).

Physical tests keep `FailFastFatalErrorHandler`; tests that move tips use
`DefaultLegPostureByPositionMap`, not the neutral map (`MaydayLegTests` is the
exception, see [Main](./Main/README.md)).

## Architectural Constraints

Do not equate skipped physical tests with successful hardware validation.

## Side Effects

- Hardware-free tests read `DefaultLegPostureByPositionMap.json` from the
  output directory.
- Physical and manual tests open the FTDI adapter (serial number `FT2H2YMW`)
  and serial port. All but `RotationDirectionOnRobotTests`, which only reads
  registers, enable servo torque and move the robot. Tests built on
  `DynamixelJointFactory` also start `PeriodicScheduler` loops that raise the test process to
  `ProcessPriorityClass.High` and request a 1 ms Windows timer resolution.
- Manual tests open a PowerShell prompt window per question
  (`ConsoleWindowHumanPrompt`, Windows only).

## Verification

- Run: `dotnet test Test\Test.csproj --filter "FullyQualifiedName~Test.Integration"`.
  This includes `Tier=Physical` and `Tier=Manual` tests, which move the
  robot: run only after the operator confirms the robot, port and operating
  conditions.
- Hardware-free: `dotnet test Test\Test.csproj --filter "FullyQualifiedName~Test.Integration&Tier!=Physical&Tier!=Manual"`.

## Common Pitfalls

[Physical runs](../../knowledge/pitfalls/physical-runs.md),
[posture map silent fallback](../../knowledge/pitfalls/posture-map-silent-fallback.md).

## Related ADRs

[Hardware boundary](../../docs/adr/0002-hardware-adapter-boundary.md),
[test tiers](../../docs/adr/0005-test-tiers-for-physical-and-simulated-runs.md),
[manual robot tests](../../docs/adr/0009-manual-robot-tests-confirmed-by-a-human.md),
[pre-push gate drives the robot](../../docs/adr/0011-pre-push-gate-drives-the-robot.md),
[right legs reverse coxa](../../docs/adr/0013-right-legs-reverse-coxa-and-mirrored-posture-lookup.md).

## Related Findings

[Posture map accuracy](../../knowledge/findings/posture-map-accuracy.md),
[right coxa direction mismatch](../../knowledge/findings/right-coxa-direction-mismatch.md).

## Related Root Causes

[Physical tests leak the serial adapter](../../knowledge/root-causes/physical-tests-leak-the-serial-adapter.md),
[tip position test used the neutral map](../../knowledge/root-causes/tip-position-test-used-neutral-map.md),
[rear femur hits the body](../../knowledge/root-causes/rear-femur-hits-body.md).

## Related Lessons Learned

[Predict on echo before hardware](../../knowledge/lessons-learned/predict-on-echo-before-hardware.md).

## Related Failed Attempts

None recorded.
