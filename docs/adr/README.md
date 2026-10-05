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
| [0005 - Test tiers for physical and simulated runs](0005-test-tiers-for-physical-and-simulated-runs.md) | Proposed | Physical versus simulated test selection |
| [0006 - Run quality gates locally](0006-run-quality-gates-locally.md) | Accepted | Local gate and hooks; GitHub CI optional |
| [0007 - Infrastructure code is tested like product code](0007-infrastructure-code-is-tested-like-product-code.md) | Accepted | Tests for scripts, hooks and CI config |
