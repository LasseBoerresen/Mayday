# Goal

Have the authoring Claude session wait for the automatic PR review and fix its findings before the owner is asked to review ([MAY-84](https://linear.app/mayday-robot/issue/MAY-84)).

## Attempt

[LasseBoerresen/Mayday#57](https://github.com/LasseBoerresen/Mayday/pull/57) (closed, 2026-11-07) kept the `PostToolUse` hook that starts `review-pr.ps1` detached, and built a wait around it: a new `wait-for-review.ps1` that waited up to 60 minutes for the review of a given head, a `-Head` option making `review-pr.ps1` wait for the pushed commit to reach the pull request, one lock per head, `<n>-<sha>.failed` and `<n>-<sha>.superseded` marker files, and hook output through `additionalContext` telling the session to run the wait. It added 976 lines over 10 files, 625 of them Pester tests, and ADR 0014 of its own.

## Why It Failed

It did not converge. Over 17 review rounds the reviewer found a new edge case almost every round, nearly all in the waiting machinery itself rather than in the behavior asked for: a background push read before its head landed, a re-push of the same commit after a failed push, stale failure markers, `git switch x && git push` or `git -C <other worktree> push` naming the wrong head, `git push && gh pr create` finding no pull request yet, the tool timeout ending the wait before the review. Each fix added state or options, and the next review found a race in those. One round said "No findings.", and later fixes reopened it.

## Evidence

The 17 review comments and the author's replies on [LasseBoerresen/Mayday#57](https://github.com/LasseBoerresen/Mayday/pull/57). The root of most findings is in [the background-task finding](../findings/claude-code-background-tasks-and-hooks.md): the hook fires when a background push starts, so a detached review had to wait for, and guess, the commit being pushed.

## Lessons Learned

Waiting on an asynchronous process that was started too early, from outside the process that knows when the work is done, needs state to bridge the gap, and that state has its own races. Start the work from the place that knows it is done instead. Reviews that grade every finding the same push the author to fix rare edge cases, which adds code to review; severity labels and a round cap bound that.

## Preferred Alternative

[ADR 0014](../../docs/adr/0014-authoring-session-follows-the-review.md): the authoring session runs `review-pr.ps1` in the background after its push succeeds and is woken when the task ends; the hook is removed; the reviewer labels findings Major or Minor; the session stops at a review with no Major findings or after 3 reviews. Design: [docs/designs/may-84-review-loop.md](../../docs/designs/may-84-review-loop.md).

## Related Components

[scripts](../../scripts/README.md)

## Related Decisions

[ADR 0010](../../docs/adr/0010-local-claude-review-of-every-pull-request.md), [ADR 0014](../../docs/adr/0014-authoring-session-follows-the-review.md)
