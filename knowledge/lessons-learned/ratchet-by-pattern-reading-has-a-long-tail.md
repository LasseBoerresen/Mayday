# A ratchet that reads C# by pattern has a long tail of bypasses

## Situation

`scripts/quarantine-ratchet.ps1` ([ADR 0011](../../docs/adr/0011-pre-push-gate-drives-the-robot.md)) refuses a push that quarantines or skips an existing test. It reads the C# files under `Test/` at `HEAD` and at the merge-base with regular expressions, rather than compiling them, to stay a few seconds long.

## Observation

Nine rounds of independent review on PR #51 each found a new way to quarantine an existing test unseen, and each fix was a parser case:

- the hand-written `[Trait("Quarantine", "true")]`
- full names, such as `QuarantineAttribute` and `TraitAttribute`
- attributes on the same line as the method
- classes of the same name in different namespaces
- a class-level quarantine covering tests added to the class
- nested types, and positional records spread over lines
- helper methods wrongly inheriting the class's quarantine
- `#pragma` lines and block comments between an attribute and its test

The gate itself decides quarantine by the xUnit trait, which the compiler and test runner see exactly; the ratchet's pattern reading approximates that and only matches it case by case.

## Recommendation

When a check has to agree with what a compiler or test runner decides, derive it from that tool's output if the cost allows, not from text patterns. Here that would mean listing `Quarantine=true` tests with `dotnet test --list-tests` for both trees, which needs the merge-base built too. This is a hypothesis: it was not tried, and listing shows method names only, so it would not see a skip by itself. If pattern reading stays, give each new bypass a red Pester case before its fix ([make tests able to fail on the bug they guard](tests-must-discriminate.md)), and list the known gaps where readers of the decision look (ADR 0011's limitations).

## Related Components

[scripts](../../scripts/README.md), [Test](../../Test/README.md)

## Related Tasks

[Physical pre-push gate](../task-summaries/physical-pre-push-gate.md)
