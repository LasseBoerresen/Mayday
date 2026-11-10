# Shared physics

## Responsibility

Model the physics that holds for any robot, such as loads, forces, mass and dynamics. Today it holds one physical quantity, `LoadRatio`; other physics concepts belong here as they are added.

## Interface

- **Load:** `LoadRatio(Ratio Value)`, an actuator's load as a UnitsNet `Ratio` (`LoadRatio.cs`). `LoadRatio.Zero` is no load; `LoadRatio.FromSteps(int steps)` gives `steps` × 0.1 %.

## Key Concepts

- **Load ratio:** the load on an actuator as a fraction of its maximum, so 100 % is full load. The sign gives the direction of the load; the actuator defines which direction is positive.
- **Units:** the value is a `Ratio`, never a bare number, so a percentage and a fraction cannot be confused.

## Dependencies

Part of `RobotDomain`. Uses `UnitsNet` (`Ratio`) only.

## Dependency Rules

Keep vendor register conversion in the adapter, not here. `FromSteps` breaks this rule (see Known Issues).

## Invariants

Preserve value semantics and physical meaning of load ratios.

- **Step size:** one step is a fixed 0.1 % (`LoadRatio.StepSize`).
- **Validation:** none. `FromSteps` neither validates nor clamps, so values beyond ±100 % pass through.
- `LoadRatio` is an immutable positional record.

## Known Issues

- `FromSteps` is a vendor register conversion inside the domain: [MAY-103: Need: RobotDomain.Physics holds no actuator register conversion](https://linear.app/mayday-robot/issue/MAY-103).
- Observed load ratios are about 0 % or about 6000 % (TODO in `LoadRatio.cs`). The likely cause is that the caller reads the signed register as unsigned, so a small negative load becomes about 6550 %; `FromSteps` passes it on unchecked: [MAY-102: Need: A negative actuator load reads as a negative LoadRatio](https://linear.app/mayday-robot/issue/MAY-102).

## Side Effects

None: `LoadRatio` is a pure value type.

## Verification

No dedicated tests. Build and run all tests: `dotnet test Test\Test.csproj`.

## Architectural Constraints

Do not substitute unitless values for domain quantities.

## Common Pitfalls

Do not conflate an actuator's raw register value with a domain load ratio.

## Related ADRs

[Stable abstractions](../../docs/adr/0001-stable-robot-abstractions.md).

## Related Findings

None recorded.

## Related Root Causes

None recorded.

## Related Lessons Learned

None recorded.

## Related Failed Attempts

None recorded.
