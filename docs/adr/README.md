# Architectural decision records

ADRs record significant decisions, the alternatives considered, and consequences. Use [the ADR template](../../templates/adr-template.md), assign the next `NNNN-short-title.md` filename, and distinguish accepted policy from proposals. Do not rewrite history: a changed decision needs a new ADR linking its predecessor; mark the old record Superseded and update the [knowledge index](../../knowledge/INDEX.md).

For decisions about dependencies, API relationships, or interactions, add or link a small editable diagram when it makes the tradeoffs clearer. Explicitly label proposed changes versus existing code; [diagram conventions](../architecture/README.md#diagram-conventions) apply.

## Decisions

| ADR | Status | Scope |
|---|---|---|
| [0001 - Stable robot abstractions](0001-stable-robot-abstractions.md) | Accepted | Dependency direction across the two robots |
| [0002 - Hardware access at the adapter boundary](0002-hardware-adapter-boundary.md) | Accepted | Device communication and test substitution |
| [0003 - Gate pull requests with one aggregate check](0003-gate-pull-requests-with-one-aggregate-check.md) | Superseded by 0006 | Merge gating and the master ruleset |
| [0004 - Quarantine failing tests with a trait](0004-quarantine-failing-tests-with-a-trait.md) | Accepted | Handling known-failing tests |
| [0005 - Test tiers for physical and simulated runs](0005-test-tiers-for-physical-and-simulated-runs.md) | Accepted, partly implemented; pre-push gating superseded by 0011 | Physical versus simulated test selection |
| [0006 - Run quality gates locally](0006-run-quality-gates-locally.md) | Accepted; pre-push hook superseded by 0011 | Local gate and hooks; GitHub CI optional |
| [0007 - Infrastructure code is tested like product code](0007-infrastructure-code-is-tested-like-product-code.md) | Accepted | Tests for scripts, hooks and CI config |
| [0008 - Cyclomatic complexity gate with CA1502](0008-cyclomatic-complexity-gate-with-ca1502.md) | Accepted | Complexity limit of 12 in the build, hooks, portable folder |
| [0009 - Manual robot tests confirmed by a human](0009-manual-robot-tests-confirmed-by-a-human.md) | Accepted | Physical tests asserted by the person watching |
| [0010 - Local Claude review of every pull request](0010-local-claude-review-of-every-pull-request.md) | Accepted | Independent, comment-only review by a local headless session; not a gate |
| [0011 - The pre-push gate drives the robot](0011-pre-push-gate-drives-the-robot.md) | Accepted | Physical tier, skips and quarantine on every code push |
| [0012 - The PR reviewer reads Linear](0012-pr-reviewer-reads-linear.md) | Accepted | Four read-only Linear tools for titled issue links; amends 0010's tool boundary |
| [0013 - Right legs reverse the coxa and look up the left-leg map mirrored in Y](0013-right-legs-reverse-coxa-and-mirrored-posture-lookup.md) | Accepted | Right coxa direction in model and servos; mirrored posture lookup |
