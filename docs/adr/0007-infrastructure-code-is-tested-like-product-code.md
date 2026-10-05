# Infrastructure code is tested like product code

## Status

Accepted

## Context

[ADR 0006](0006-run-quality-gates-locally.md) moved the quality gate into PowerShell scripts and hooks. Everything else relies on them: if the gate silently skips a step or reports a pass it did not earn, every later change is judged by a broken instrument. The first version of those scripts was checked only by hand, which is the situation product code is not allowed to be in. The owner's reasoning: the repository keeps two independent ledgers of behavior, code and tests (double-entry bookkeeping), and that is what keeps it robust and productive over time. A script with no ledger entry has only one record of what it should do.

## Decision

Scripts, hooks, CI configuration and other infrastructure code get tests by the same rules as product code: outside-in, a behavior change goes red then green, and a refactor leaves the tests untouched. For PowerShell the tests are Pester 5+ files in `scripts/Tests`, named `Given<State>_When<Action>_Then<Outcome>`. The gate runs them, so they cannot rot. A script gets a small seam (an optional parameter whose default is the real behavior) where a test needs to replace an expensive or dangerous step, and the seam is added in its own refactor commit. The rule is stated in the Testing section of `CLAUDE.md`.

## Alternatives Considered

- Verify scripts by running them: found nothing wrong at first, and would not notice a later regression in pass memory or exit codes.
- Keep the scripts trivial enough not to need tests: the gate has real logic (a tree fingerprint, remembered passes, exit-code propagation).
- Test only through the real solution build: 20 seconds per case and no way to simulate a failing build.

## Why

The tests were checked the way [tests must discriminate](../../knowledge/lessons-learned/tests-must-discriminate.md) asks: breaking the script in three ways (the Markdown exclusion, the robot flag, the pass memory) failed tests each time. Running the tests inside the gate also exposed a defect that running them alone did not ([lesson](../../knowledge/lessons-learned/infrastructure-tests-run-in-a-different-environment.md)).

## Consequences

- Benefits: regressions in the gate are caught by the gate; scripts can be refactored with confidence; the same discipline everywhere.
- Costs: the gate takes about 15 seconds longer when the tree changed; Pester 5+ is a prerequisite (`Install-Module Pester -MinimumVersion 5.0 -Scope CurrentUser`).
- Limitations: the real `dotnet build` and `dotnet test` commands, the hooks' wiring in Claude Code and git, and `ci.yml` are not covered by automated tests; they are exercised by use. The quarantined-test report in `gate.ps1` is not covered.

## Related Components

[scripts](../../scripts/README.md), [Test](../../Test/README.md)

## Related ADRs

[0006](0006-run-quality-gates-locally.md)

## Related Findings

None recorded.

## Related Root Causes

None recorded.
