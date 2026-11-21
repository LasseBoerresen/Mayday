# MaydayDataAccess unit tests

## Overview

Unit tests for `LegPostureByPositionMapFileRepo` of
[MaydayDataAccess](../../../MaydayDataAccess/README.md)
(namespace `Test.Unit.MaydayDataAccess`): loading a leg posture-by-position map
from a JSON file. The class also holds a by-hand builder of a new map.

## Interface

### `LegPostureByPositionMapFileRepoTests`

Checks that a map file loads into a working `LegPostureByPositionMap`, and
that an empty file is rejected.
File: `LegPostureByPositionMapFileRepoTests.cs`.

| Test | What it checks |
|---|---|
| `GivenMapFileWithOneMappingAndNeighbor__WhenLoad__ThenMapShouldUseThatMapping` | `Load()` of `SingleLegPostureByPositionMap.json` gives a map whose `GetFor(Xyz(0,0,0), …)` returns `MaydayLegPosture.FromRevolutions(0.1, 0.2, 0.3)`. |
| `GivenEmptyMapFile__WhenLoad__ThenThrowsEmptyMapException` | `Load()` of `EmptyLegPostureByPositionMap.json` throws `LegPostureByPositionMapDictImpl.EmptyMapException`. |
| `RebuildDictLegPostureByPositionMap` | **Manual, not a test.** Trait `Tier=Manual` and `[Fact(Skip = …)]`, so it never runs unless the `Skip` is removed by hand. Builds a new map with `LegPostureByPositionMapDictImpl.BuildNew()`, prints it and stores it with `Store()`. |

### Test data (`TestData/`)

JSON maps keyed by `"x,y,z"` cell position (meters), each holding a list of
postures (`CoxaAngle`, `FemurAngle`, `TibiaAngle`, revolutions).

| File | What it holds |
|---|---|
| `TestData/SingleLegPostureByPositionMap.json` | Two cells: `0.0,0.0,0.0` with posture 0.1, 0.2, 0.3, and its neighbor `-0.015625,-0.015625,-0.015625` with 0.4, 0.5, 0.6. |
| `TestData/EmptyLegPostureByPositionMap.json` | An empty JSON object. |

## Invariants

- The tests read the data files from the build output,
  `AppContext.BaseDirectory/Unit/MaydayDataAccess/TestData`, not from the
  source tree. `Test.csproj` copies both files there (`CopyToOutputDirectory`
  `PreserveNewest`); a new data file needs the same entry.
- The second cell in the single-mapping file is one map cell (1/64 m,
  `CellSize` in `LegPostureByPositionMapDictImpl`) away on each axis, the
  "neighbor" in the test name. The expected posture is the origin cell's.
- `RebuildDictLegPostureByPositionMap` must keep its `Tier=Manual` trait while
  it is skipped: the pre-push gate fails on a skipped test outside
  `Tier=Manual` (XML doc on the method).

## Side Effects

- File reads: the two JSON files in the output directory.
- `RebuildDictLegPostureByPositionMap`, only when run by hand: writes
  `DefaultLegPostureByPositionMap.json` to the output `TestData` directory
  (to be copied into the `MaydayDataAccess` project by hand) and prints every
  cell to `ITestOutputHelper`.
- No hardware or user secrets.

## Verification

- Run: `dotnet test Test\Test.csproj --filter "FullyQualifiedName~Test.Unit.MaydayDataAccess"`
  (reports `RebuildDictLegPostureByPositionMap` as skipped).
- Without the manual builder: `dotnet test Test\Test.csproj --filter "FullyQualifiedName~Test.Unit.MaydayDataAccess&Tier!=Manual"`
