# MaydayDomain integration test object mother

## Overview

Builders for the leg posture maps and echo structures that integration tests
use with [MaydayDomain](../../../../MaydayDomain/README.md) and
[MaydayDataAccess](../../../../MaydayDataAccess/README.md). It holds no tests.

## Interface

### `TestObjectMother`

Gives tests a posture map, or a hardware-free leg or structure of echo joints.
Class in namespace `Test.Integration.MaydayDomain.Base`, implementing the
marker interface `Test.Unit.Base.TestObjectMother`. File: `TestObjectMother.cs`.

| Member | What it does |
|---|---|
| `LegPostureByPositionMap` | A new neutral map, `LegPostureByPositionMapDictImpl.CreateNeutral()`: one mapping, origin to `MaydayLegPosture.Neutral`. Any other tip position falls back to the current posture. |
| `DefaultLegPostureByPositionMap` | The map the robot uses, loaded once from `DefaultLegPostureByPositionMap.json` in `AppContext.BaseDirectory` with `LegPostureByPositionMapFileRepo.ForDefaultMapIn(...).Load()`. |
| `CreateEchoStructureWithDefaultMap()` | A new `DefaultMaydayStructure` from `MaydayStructureFactory.CreateDefault()` with `EchoJointFactory` joints and the default map. |
| `CreateEchoLegWithDefaultMap(legId)` | A new `MaydayLeg` for `legId` from `MaydayLegFactory` with `EchoJointFactory` joints and the default map. |

All members are `internal static`. Users: the tests in
[MaydayDomain](../README.md), [Dynamixel](../../Dynamixel/README.md),
[Main](../../Main/README.md), [Robots/Base](../../Robots/Base/README.md), and
unit tests in `Test/Unit/MaydayDomain`.

## Invariants

- A test that moves tips or looks up postures uses
  `DefaultLegPostureByPositionMap` (or a map built for its targets), never the
  neutral map: the neutral map finds a posture only for the origin
  ([root cause](../../../../knowledge/root-causes/tip-position-test-used-neutral-map.md),
  [pitfall](../../../../knowledge/pitfalls/posture-map-silent-fallback.md)).
- The neutral map has one mapping because `LegPostureByPositionMapDictImpl`
  rejects an empty map
  ([root cause](../../../../knowledge/root-causes/empty-leg-posture-map.md)).
- `DefaultLegPostureByPositionMap` is shared by every test in the process
  (`Lazy`); `LegPostureByPositionMap` and the echo builders return new
  instances on each call.
- The default map is read from the test output directory, where
  `MaydayDataAccess.csproj` copies the JSON file; a missing or empty file
  throws on first use.

## Side Effects

- `DefaultLegPostureByPositionMap` and the echo builders read
  `DefaultLegPostureByPositionMap.json` from disk, once per test process.
- Otherwise none: echo joints touch no hardware.

## Verification

- No tests of its own. Its users run with:
  `dotnet test Test\Test.csproj --filter "FullyQualifiedName~Test.Integration.MaydayDomain&Tier!=Physical&Tier!=Manual"`
  (hardware-free).
