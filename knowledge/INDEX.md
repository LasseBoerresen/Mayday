# Repository Knowledge Index

Start here before significant work. Read [CLAUDE.md](../CLAUDE.md), the affected [component READMEs](#component-directory), and the underlying records; this index is a map, not a substitute for evidence. Search all record directories for related concepts. Whenever significant knowledge is added or an ADR changes status, update this index and the corresponding directory README in the same change. Keep links and "recent" lists current; do not invent history to populate a section.

## Most Important Architectural Decisions

- [ADR 0001: Stable robot abstractions](../docs/adr/0001-stable-robot-abstractions.md)
- [ADR 0002: Hardware access at the adapter boundary](../docs/adr/0002-hardware-adapter-boundary.md)
- [ADR 0003: Gate pull requests with one aggregate check](../docs/adr/0003-gate-pull-requests-with-one-aggregate-check.md)
- [ADR 0004: Quarantine failing tests with a trait](../docs/adr/0004-quarantine-failing-tests-with-a-trait.md)
- [ADR 0005: Test tiers for physical and simulated runs](../docs/adr/0005-test-tiers-for-physical-and-simulated-runs.md) (Proposed, not implemented)

## Architectural Constraints

- Dependencies should point toward stable shared contracts, not from `RobotDomain` into robot-specific projects ([architecture map](../docs/architecture/README.md)). The current `MaydayDomain` hardware reference is a [documented exception](findings/mayday-domain-hardware-reference.md).
- Physical tests and startup can move real hardware: [physical-run pitfall](pitfalls/physical-runs.md).
- Port access must remain serialized and scheduler changes require timing/safety review ([Dynamixel](../Dynamixel/README.md), [RobotDomain/Time](../RobotDomain/Time/README.md)).
- Merges to `master` require the aggregate `gate` check ([ADR 0003](../docs/adr/0003-gate-pull-requests-with-one-aggregate-check.md)). A test known to fail is quarantined with a recorded reason, not skipped ([ADR 0004](../docs/adr/0004-quarantine-failing-tests-with-a-trait.md)).

## Known Pitfalls

- [Physical runs are not ordinary tests](pitfalls/physical-runs.md)
- [Using measured angle as every interpolation baseline](pitfalls/interpolation-baseline.md)
- [Periodic loops in tests](pitfalls/periodic-loops-in-tests.md)
- [Q's `-` operator returns inverse(a)·b](pitfalls/q-minus-operator-direction.md)

## Common Root Causes

- [Interpolation drift from a measured-angle baseline](root-causes/interpolation-baseline-drift.md)
- [Test host crash from scheduler FailFast](root-causes/test-host-crash-from-scheduler-failfast.md)
- [Step angle conversion used 4,094 counts per revolution](root-causes/step-angle-4094-counts.md)
- [Empty leg posture map rejected by its own callers](root-causes/empty-leg-posture-map.md)
- [Tests that passed locally failed on a clean CI runner](root-causes/tests-failing-only-on-a-clean-runner.md)
- [Q.Rotate rejected positions longer than 1.1 m](root-causes/q-rotate-wrapped-position-in-q.md)
- [Transform.HalfWayTo returned the wrong transform](root-causes/transform-halfway-to-wrong-result.md)

## Frequently Used Findings

- [MaydayDomain currently references Dynamixel](findings/mayday-domain-hardware-reference.md)
- [PeriodicScheduler is constructed inside four production classes](findings/scheduler-constructed-inside-domain-classes.md)
- [PeriodicScheduler.Wait ignores cancellation](findings/periodic-scheduler-wait-ignores-cancellation.md)
- [PeriodicScheduler cannot be stepped under a fake clock](findings/periodic-scheduler-not-steppable-under-fake-time.md)

## Recent Lessons Learned

- [Separate trajectory goals from measured joint state](lessons-learned/goal-versus-measured-state.md)
- [Make tests able to fail on the bug they guard](lessons-learned/tests-must-discriminate.md)

## Recently Completed Tasks

- [Gate pull requests and establish a test baseline](task-summaries/pr-gating-and-test-baseline.md)
- [Migrate agent guidance to CLAUDE.md](task-summaries/claude-md-migration.md)
- [Repository memory foundation](task-summaries/repository-memory-foundation.md)

## Component Directory

| Area | Documentation |
|---|---|
| Shared | [Generic](../Generic/README.md) ([System](../Generic/System/README.md)), [RobotDomain](../RobotDomain/README.md) ([Behavior](../RobotDomain/Behavior/README.md), [Geometry](../RobotDomain/Geometry/README.md), [System.Numerics](../RobotDomain/Geometry/SystemNumerics/README.md), [Motion](../RobotDomain/Motion/README.md), [Physics](../RobotDomain/Physics/README.md), [Structures](../RobotDomain/Structures/README.md), [Time](../RobotDomain/Time/README.md)) |
| Mayday | [MaydayDomain](../MaydayDomain/README.md) ([Components](../MaydayDomain/Components/README.md), [MotionPlanning](../MaydayDomain/MotionPlanning/README.md)), [ManualBehavior](../ManualBehavior/README.md), [MaydayDataAccess](../MaydayDataAccess/README.md) ([Geometry](../MaydayDataAccess/Geometry/README.md)), [Robots](../Robots/README.md), [Main](../Main/README.md) |
| Hardware | [Dynamixel](../Dynamixel/README.md) |
| Ellie | [Ellie](../Ellie/README.md): [EllieMain](../Ellie/EllieMain/README.md) ([Base](../Ellie/EllieMain/Base/README.md), [Behaviors](../Ellie/EllieMain/Behaviors/README.md), [MotionPlanning](../Ellie/EllieMain/MotionPlanning/README.md), [Structures](../Ellie/EllieMain/Structures/README.md)), [EllieMainTests](../Ellie/EllieMainTests/README.md) |
| Tests and other projects | [Test](../Test/README.md) ([Unit](../Test/Unit/README.md), [Integration](../Test/Integration/README.md)), [DataAccess](../DataAccess/README.md), [MauiApp1](../MauiApp1/README.md), [Ternimal](../Ternimal/README.md) |

## Documentation Structure

- [Root overview](../README.md) and [architecture map](../docs/architecture/README.md): intent and dependencies.
- [ADRs](../docs/adr/README.md): decisions and supersession history.
- [Knowledge categories](#where-to-record-new-knowledge): evidence, mistakes, and work history.
- [Templates](../templates/README.md): required record format.
- [Diagram conventions and architecture example](../docs/architecture/README.md#diagram-conventions): how to document relationships and interactions visually without obscuring current implementation.

## Where To Record New Knowledge

| New knowledge | Location |
|---|---|
| Decision, alternatives, deviation | [ADRs](../docs/adr/README.md) |
| Discovery supported by evidence | [Findings](findings/README.md) |
| Diagnosed failure and prevention | [Root causes](root-causes/README.md) |
| Transferable insight | [Lessons learned](lessons-learned/README.md) |
| Recurring hazard | [Pitfalls](pitfalls/README.md) |
| Abandoned approach and why | [Failed attempts](failed-attempts/README.md) |
| Significant completed work | [Task summaries](task-summaries/README.md) |
| Changed responsibility or API | Affected component README |
