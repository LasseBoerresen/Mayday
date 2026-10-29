# Findings

Record important, evidence-backed discoveries that have not necessarily caused a failure. Copy [the finding template](../../templates/finding-template.md); cite code, tests, or historical notes, mark uncertain conclusions as such, and link affected components and decisions. Add significant findings here and to the [index](../INDEX.md).

## Entries

- [MaydayDomain currently references Dynamixel](mayday-domain-hardware-reference.md)
- [PeriodicScheduler is constructed inside four production classes](scheduler-constructed-inside-domain-classes.md)
- [PeriodicScheduler.Wait ignores cancellation](periodic-scheduler-wait-ignores-cancellation.md)
- [PeriodicScheduler cannot be stepped under a fake clock](periodic-scheduler-not-steppable-under-fake-time.md)
- [Posture map lookups are accurate to about 1 cm](posture-map-accuracy.md)
- [The model and the robot disagreed on the right coxa direction (resolved, ADR 0013)](right-coxa-direction-mismatch.md)
- [Worktrees share the main checkout's hooks](absolute-hooks-path-shared-by-worktrees.md)
