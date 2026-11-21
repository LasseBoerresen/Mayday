# Robots integration test object mother

## Overview

Gives tests the real [Robots](../../../../Robots/README.md) factory for
Mayday. It holds no tests, but what it builds opens the serial port when its
methods run.

## Interface

### `TestObjectMother`

Builds a `MaydayRobotFactory` for tests. Class `internal` in namespace
`Test.Integration.Robots.Base`. File: `TestObjectMother.cs`.

| Member | What it does |
|---|---|
| `MaydayRobotFactory` | A new `MaydayRobotFactory` with the neutral map (`Test.Integration.MaydayDomain.Base.TestObjectMother.LegPostureByPositionMap`, see [MaydayDomain/Base](../../MaydayDomain/Base/README.md)) and a `FailFastFatalErrorHandler`. |

Users: `Test.Integration.Main.MaydayRobotTests` ([Main](../../Main/README.md))
and `Test/Unit/Robots/Mayday/MaydayDayRobotFactoryTests.cs`.

## Invariants

- It is the production hardware factory, so it keeps the production
  fail-fast handler: an exception in a periodic loop ends the test process.
  Do not use it in a hardware-free test that runs its effects.
- The neutral map holds one posture, for the origin: robots from this
  factory cannot place tips through posture lookups.

## Side Effects

- None when the property is read. Every `MaydayRobotFactory.Create...`
  method calls `DynamixelJointFactory.Create`, which touches the FTDI adapter
  when it is plugged in (opens it, sets latency, reads the COM port name,
  closes it) while the effect is built; running the effect opens the serial port and initializes
  the joints.

## Verification

- No tests of its own. See [Main](../../Main/README.md) for the physical
  tests that use it.
