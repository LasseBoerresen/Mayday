# Test tiers for physical and simulated runs

## Status

Proposed. The decision was made, but the `Tier` trait is not implemented yet.

## Context

`PhysicalRobotFact` and `PhysicalRobotTheory` mark tests that drive the real robot. They set no trait. They skip themselves at run time when `MAYDAY_ROBOT_IS_CONNECTED` is not `True` (`Test/Utilities/PhysicalRobotFactAttribute.cs`, `TestConfiguration.cs`), so CI cannot tell which tier a test belongs to, only that it happened to skip. The physical tests are meant to evolve into simulation, with Gazebo or Isaac Sim as candidates. A test that moves a leg should eventually run unchanged against hardware or a simulator.

## Decision

Add a `Tier` trait now. `PhysicalRobotFact` and `PhysicalRobotTheory` set `Tier=Physical`, and CI filters on it explicitly. The run-time skip stays as a safety net. When simulation arrives, a test's tier changes from `Physical` to `Simulated` and the robot comes from the test environment through constructor injection. The longer direction is environment-selected tiers (`Unit`, `Integration`, `Simulation`, `Hardware`). Hardware tests run only in a separate, manually triggered workflow on a self-hosted runner, never as a merge gate.

## Alternatives Considered

- A separate `SimulatedRobotFact` beside `PhysicalRobotFact`: tests are duplicated or rewritten when they migrate.
- Tiers by environment only, from the start: more change now, with no simulator to select yet.

## Why

An explicit trait makes the gate honest about what ran. Simulation then becomes a later tier instead of a rewrite. Gazebo can run headless in a container on a GitHub-hosted runner, so a fast simulated tier could join the gate. Isaac Sim needs an NVIDIA GPU, so a heavy tier would be nightly or on demand, outside the gate.

## Consequences

- Benefits: explicit CI filters; a migration path from hardware to simulation.
- Costs: the trait and the manual hardware workflow remain to be built.
- Limitations: a self-hosted runner on a public repository is a security risk, so the hardware workflow must stay manual and restricted to the owner. The [physical-runs pitfall](../../knowledge/pitfalls/physical-runs.md) still applies.

## Related Components

[Test](../../Test/README.md), [Dynamixel](../../Dynamixel/README.md)

## Related ADRs

[0002](0002-hardware-adapter-boundary.md), [0003](0003-gate-pull-requests-with-one-aggregate-check.md), [0004](0004-quarantine-failing-tests-with-a-trait.md)

## Related Findings

None recorded.

## Related Root Causes

None recorded.
