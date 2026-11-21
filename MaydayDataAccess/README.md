# MaydayDataAccess

## Overview

Mayday's persistence adapter. It implements `MaydayDomain`'s
`LegPostureByPositionMapRepo` with a JSON file, and ships the posture map the
robot uses, `DefaultLegPostureByPositionMap.json`. `Main` loads that map at
startup and hands it to the `Robots` factory; the integration tests load the
same copy.

## Responsibility

Persist and map Mayday leg posture-by-position data.

## Public APIs

`LegPostureByPositionMapFileRepo` (with `ForDefaultMapIn` for the map the robot uses, `DefaultLegPostureByPositionMap.json`). The DAOs `MaydayLegPostureDao` and `Geometry.XyzDao` are `internal`.

## Interface

Namespace `MaydayDataAccess`.

### `LegPostureByPositionMapFileRepo`

Loads and stores a `LegPostureByPositionMap` as a JSON file.
`LegPostureByPositionMapFileRepo(FileInfo mapFileInfo)`, implements
`MaydayDomain.LegPostureByPositionMapRepo`. File:
`LegPostureByPositionMapFileRepo.cs`.

| Member | What it does |
|---|---|
| `DefaultMapFileName` | `"DefaultLegPostureByPositionMap.json"`, the file name of the map the robot uses. |
| `ForDefaultMapIn(DirectoryInfo)` | A repo for `DefaultMapFileName` in that directory. `Main` and the tests' `TestObjectMother` pass `AppContext.BaseDirectory`. |
| `Load()` | Reads the whole file and returns it as a `LegPostureByPositionMap` (at runtime a `LegPostureByPositionMapDictImpl` backed by a frozen dictionary). |
| `Store(LegPostureByPositionMapDictImpl)` | Writes the map to the file as indented JSON, sorted by tip position. |

### File format

One JSON object. Each key is a tip position in meters, `"<X>,<Y>,<Z>"` (see
[Geometry](./Geometry/README.md)); each value is a list of the postures that
put the tip there, with angles in revolutions:

```json
{
  "-0.203125,-0.0625,0": [
    { "CoxaAngle": 0.046875, "FemurAngle": -0.4, "TibiaAngle": 0.303125 }
  ]
}
```

### Internal types

- `MaydayLegPostureDao(double CoxaAngle, double FemurAngle, double TibiaAngle)`
  (`MaydayLegPostureDao.cs`): one posture in revolutions. `ToDomain()` calls
  `MaydayLegPosture.FromRevolutions`; `FromDomain` takes each angle's
  `Revolutions`.
- `XyzDao`: the position key, documented in [Geometry](./Geometry/README.md).

### Non-code assets

`DefaultLegPostureByPositionMap.json` (about 5 MB), built by
`LegPostureByPositionMapDictImpl.BuildNew()`. The project file copies it to the
output directory (`PreserveNewest`), so it also lands beside the binaries of
every project that references this one.

## Key Concepts

JSON string keys converted to units-aware position keys and frozen lookup maps.

## Dependencies

`MaydayDomain`, `RobotDomain`, and the `LanguageExt.Sys` package (referenced,
but not used directly by this project's code); see `MaydayDataAccess.csproj`.
The `Map` extension on sequences comes from `Generic`, reached through
`RobotDomain`.

## Dependency Rules

File formats and DAO conversion stay in persistence, not robot structure abstractions.

## Invariants

Loading requires a readable, valid map; storing orders coordinates for stable output.

`Load` throws, and returns no map, for these inputs:

| Input | Throws |
|---|---|
| The file is missing or unreadable | From `File.ReadAllText`: an `IOException` (such as `FileNotFoundException`), or `UnauthorizedAccessException` when access is denied. |
| The content is not valid JSON, or not an object of posture lists | `JsonException`. |
| The content is JSON `null` | `InvalidOperationException`. |
| A key's value is `null` | `ArgumentNullException`. |
| A key is not three invariant-culture numbers | `FormatException`, from `XyzDao.FromKey`. |
| A key has a `NaN` coordinate, such as `"NaN,0,0"` | `ArgumentException`, from the `Xyz` constructor. |
| Two keys parse to the same position, such as `"0,0,0"` and `"0.0,0,0"` | `ArgumentException`, from `ToDictionary`. |
| The object is empty (`{}`) | `LegPostureByPositionMapDictImpl.EmptyMapException`. |

`Load` does not check the content further, and loads these without error:

| Input | What loads |
|---|---|
| A posture missing an angle, or naming it in another case (`coxaAngle`) | That angle as 0 revolutions: property names are case-sensitive. |
| The same key text twice | The last entry only. |
| An empty posture list (`[]`) | The cell; `LegPostureByPositionMapDictImpl.GetFor` throws `NotSupportedException` when it looks that cell up. |
| A key off the 1/64 m cell grid, including an infinite coordinate (`"Infinity,0,0"`) | The entry, but `GetFor` looks up cell centers only, so it is never found and the lookup keeps the current posture. |

`Store` sorts entries by X, then Y, then Z, in meters, so the same map always
gives the same file. A map written by `Store` loads back to the same positions
and angles.

## Side Effects

- `Load` reads the file; `Store` creates or overwrites it with
  `File.WriteAllText`. Neither locks the file or creates the directory.
- No other state: the repo holds only its `FileInfo`.

## Architectural Constraints

The application supplies the file path and owns file availability.

## Common Pitfalls

JSON object keys are strings; do not deserialize directly into complex coordinate keys. See [posture lookups that silently keep the current posture](../knowledge/pitfalls/posture-map-silent-fallback.md).

`Main` calls `Load` without handling its exceptions (a `TODO` in
`CreateMaydayRobotFactory`), so a missing or invalid map stops startup.

The map is rebuilt by hand: the skipped, `Tier=Manual` test
`LegPostureByPositionMapFileRepoTests.RebuildDictLegPostureByPositionMap`
stores a new map in
`<test output>\Unit\MaydayDataAccess\TestData\DefaultLegPostureByPositionMap.json`,
and that file must then be copied over `DefaultLegPostureByPositionMap.json` in
this project.

## Verification

- Run: `dotnet test Test\Test.csproj --filter "FullyQualifiedName~Test.Unit.MaydayDataAccess"`
  (hardware-free; loads the small maps in `Test\Unit\MaydayDataAccess\TestData`).
- `Test.Integration.MaydayDomain` tests load `DefaultLegPostureByPositionMap.json`
  from the test output directory through `ForDefaultMapIn`.

## Related ADRs

[Stable abstractions](../docs/adr/0001-stable-robot-abstractions.md).

## Related Findings

[Posture map accuracy](../knowledge/findings/posture-map-accuracy.md).

## Related Root Causes

None recorded.

## Related Lessons Learned

None recorded.

## Related Failed Attempts

None recorded.
