# RobotDomain

The stable core every robot builds on ([ADR 0001](../docs/adr/0001-stable-robot-abstractions.md)). Read [README.md](README.md) and the README of the affected sub-namespace (`Behavior`, `Geometry`, `Motion`, `Physics`, `Structures`, `Time`).

## Boundary

- Model concepts that hold for any robot: joints, links, structures, geometry, motion contracts, timing. Mayday concepts (legs, postures, hexapod kinematics) belong in `MaydayDomain`; Ellie concepts belong in `Ellie\EllieMain`.
- References point only to `Generic` and general-purpose libraries; robot projects, executables and hardware libraries depend on this project, never the reverse.
- Changes here ripple into both robots: build `Mayday.sln` and run both `Test\Test.csproj` and `Ellie\EllieMainTests\EllieMainTests.csproj`.

## Timing (`Time`)

- `PeriodicScheduler` drives the periodic loops. It raises process priority to `High` and hands unobserved callback errors to the `FatalErrorHandler` passed to its constructor; composition roots pass `FailFastFatalErrorHandler`, which calls `Environment.FailFast`. Keep the priority and the fail-fast in production unless the change is about them, and give each periodic planner or driver its handler explicitly from the composition root. Changes to cadence, timer resolution, cancellation, shutdown or priority affect physical motion: state the timing and safety reasoning in the commit `Rationale:`.
- Planners and drivers take a `TimeProvider` from their caller so tests can control time.
- `Motion.PeriodicallyBatchedJointDriver` runs at a higher rate than behavior and planning; see `Dynamixel/CLAUDE.md` for the port-serialization rules it relies on.
