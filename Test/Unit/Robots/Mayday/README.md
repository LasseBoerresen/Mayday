# Unit tests of the Mayday robot factory

## Overview

Tests `MaydayRobotFactory` of the [Robots](../../../../Robots/README.md)
project (namespace `Test.Unit.Robots.Mayday`). The one test builds the effect
that creates a Mayday robot with the sway behavior but never runs it, so it
checks little.

## Interface

### `MaydayDayRobotFactoryTests`

Checks that `MaydayRobotFactory.CreateWithSwayBehavior` returns an effect.
File: `MaydayDayRobotFactoryTests.cs`.

| Test | What it checks |
|---|---|
| `GivenMaydayRobotFactory_WhenCreateWithSwayBehavior__ThenIsNotNull` | `CreateWithSwayBehavior(TimeProvider.System)` returns without throwing. The `NotBeNull` assertion sits inside `Map` on the returned `Eff<MaydayRobot>`, which the test never runs, so the assertion never executes. |

The factory comes from `Test.Integration.Robots.Base.TestObjectMother.MaydayRobotFactory`:
the real factory with the neutral leg posture map
(`Test.Integration.MaydayDomain.Base.TestObjectMother.LegPostureByPositionMap`)
and `FailFastFatalErrorHandler`. A `TODO` in the file plans a test that the
robot gets the right behavior controller.

## Invariants

- The test is a plain `[Fact]` with no `Tier` trait, so the Stop hook's gate
  runs it.
- It never runs the effect: no COM port is opened, no joint is created and
  the robot does not move (building it does reach the FTDI adapter, see Side
  Effects).

## Side Effects

- Building the effect is not hardware-free. `DynamixelJointFactory.Create`
  calls `NativeSerialPortCommunicationBus.CreateInitialized`, whose
  `ConfigurePortLatencyAndGetComPortName` runs immediately: it opens the FTDI
  adapter with serial number `FT2H2YMW`, sets its latency to 1 ms, reads its
  COM port name and closes it again. Without the adapter,
  `OpenBySerialNumber` returns a failure status and the method returns an
  error effect instead of throwing, which the test does not inspect.

## Verification

- Run: `dotnet test Test\Test.csproj --filter "FullyQualifiedName~Test.Unit.Robots.Mayday"`.
  No `Tier=Physical` or `Tier=Manual` tests are included, but see Side Effects:
  with the robot's adapter plugged in, the test opens and configures it.
