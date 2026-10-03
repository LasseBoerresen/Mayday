# Gate pull requests and establish a test baseline

## Problem

`master` had no automated check, and the `Test` project could not serve as one: the test host crashed partway, hiding results, and once it ran to completion 46 of 212 tests failed. The goal is a merge gate that grows from build to tests, coverage, complexity and mutation testing, and that applies equally to agent-authored changes.

## Solution

- **Gate:** `.github/workflows/ci.yml` with a `build` job and an aggregate `gate` job (PR #20, `b28bfb8`), and the ruleset "master gate" (id 24423412) requiring `gate`. See [ADR 0003](../../docs/adr/0003-gate-pull-requests-with-one-aggregate-check.md).
- **Crash:** the test host no longer crashes: `813fa03` and `d69284f`. See [the root cause](../root-causes/test-host-crash-from-scheduler-failfast.md).
- **Quarantine:** `[Quarantine("reason")]` and 46 tagged tests (`a1a43c8`). See [ADR 0004](../../docs/adr/0004-quarantine-failing-tests-with-a-trait.md).
- **Empty map:** a non-empty map for tests (`5db9c38`) and production `CreateNeutral` for the echo factory (`abf6a02`). 36 tests left quarantine, then two more. See [the root cause](../root-causes/empty-leg-posture-map.md).
- **Step angle:** the 4,096-counts correction, pull request #26, merged as `88cc705`. See [the root cause](../root-causes/step-angle-4094-counts.md).

- **Test job:** `test` and `quarantined` jobs (`4326f74`) and `test` required by the gate (`69f34c4`). Their first clean-runner runs failed 8 tests; fixed by `f6b788e` and `c756cba`. See [the root cause](../root-causes/tests-failing-only-on-a-clean-runner.md).

State after `88cc705`: `dotnet test Test --filter "Quarantine!=true"` gives 209 passed, 9 skipped, 0 failed, identical across repeated runs. Eight tests stay quarantined.

## Decisions Made

- One aggregate required check, no owner bypass, PRs for everything, rebase merges only.
- Tests join the gate only once green. Failing tests are quarantined by trait, not skipped.
- Coverage and complexity start as ratchets from their current level, kept in a committed thresholds file raised by hand. Complexity limits are strict everywhere with a suppression baseline. Mutation testing starts non-blocking.
- Agent runs go through the same `gate`, trigger only on `@claude` mentions or manual dispatch restricted to the owner, with spend caps, and get stricter thresholds. A `CODEOWNERS` file and a check protect the gate files from agent edits. A Claude Code hook runs build and unit tests on `Stop` and before `git push`. All of this is planned.
- Test tiers (`Tier=Physical`, later simulated) per [ADR 0005](../../docs/adr/0005-test-tiers-for-physical-and-simulated-runs.md), Proposed.
- A fatal-error handler passed through the constructors ([finding](../findings/scheduler-constructed-inside-domain-classes.md)), planned.

## Tradeoffs

- The owner can still edit the ruleset in GitHub settings, so "no bypass" is about the normal path.
- Quarantine can hide a regression if it is tagged carelessly, which is why gate-file protection is planned.
- The `StepAngle` correction changes real motion by up to about 0.09 degrees and was verified only by unit tests.

## Follow-up Work

- `EllieMainTests` has two failures that exist on `master` and is not run in CI while Ellie is work in progress.
- Diagnose the eight quarantined tests: three `Q` validation failures, a Moq proxy mismatch, the stale driver test, a `TransformTests` mismatch, the inverse-kinematics mismatch, and the lean mismatch.
- Implement the `Tier` trait, the fatal-error handler, the quality gates, the agent workflow and its protections.
- `Main` has compiler warnings (for example an unused local function) because it does not set `TreatWarningsAsErrors`; a stricter `-warnaserror` build also fails in `EllieMain`.

## Related ADRs

[0003](../../docs/adr/0003-gate-pull-requests-with-one-aggregate-check.md), [0004](../../docs/adr/0004-quarantine-failing-tests-with-a-trait.md), [0005](../../docs/adr/0005-test-tiers-for-physical-and-simulated-runs.md)

## Related Findings

[PeriodicScheduler is constructed inside four production classes](../findings/scheduler-constructed-inside-domain-classes.md), [Wait ignores cancellation](../findings/periodic-scheduler-wait-ignores-cancellation.md)

## Related Root Causes

[Test host crash](../root-causes/test-host-crash-from-scheduler-failfast.md), [step angle](../root-causes/step-angle-4094-counts.md), [empty leg posture map](../root-causes/empty-leg-posture-map.md), [clean-runner failures](../root-causes/tests-failing-only-on-a-clean-runner.md)

## Related Lessons Learned

[Make tests able to fail on the bug they guard](../lessons-learned/tests-must-discriminate.md)
