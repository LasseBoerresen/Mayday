# Main

## Responsibility

Mayday executable startup and selection of the configured robot behavior.

## Public APIs

Executable entry point in `Program.cs`; `StartupMode` selects Run or Train.

## Key Concepts

`mayday_startup_mode`, `DefaultLegPostureByPositionMap.json`, `MaydayRobotFactory`.

## Dependencies

`Dynamixel`, `Generic`, `ManualBehavior`, `MaydayDomain`, `RobotDomain`, `Robots`.

## Dependency Rules

Own concrete startup configuration; do not move startup IO into shared domains.

## Invariants

Startup reports a missing/invalid mode; it loads the map from the application base directory.

## Architectural Constraints

Starting the executable can open a port and move real actuators.

## Common Pitfalls

[Physical runs](../knowledge/pitfalls/physical-runs.md); verify environment and map before execution.

## Related ADRs

[Hardware boundary](../docs/adr/0002-hardware-adapter-boundary.md).

## Related Findings

None recorded.

## Related Root Causes

None recorded.

## Related Lessons Learned

None recorded.

## Related Failed Attempts

None recorded.
