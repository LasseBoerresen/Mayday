# The authoring session waits for each PR review and fixes the main findings

* **Author:** LasseBoerresen (brainstorm with Claude)
* **Date:** 2026-11-07
* **Issue:** [MAY-84: Need: The automatic code review comments are addressed before agent asks me to review.](https://linear.app/mayday-robot/issue/MAY-84)
* **Replaces:** [LasseBoerresen/Mayday#57](https://github.com/LasseBoerresen/Mayday/pull/57) (closed)
* **Status:** Implemented ([ADR 0014](../adr/0014-authoring-session-follows-the-review.md)).

## Overview

Every push of a pull request gets a review from a separate, headless Claude session (`scripts/review-pr.ps1`, ADR
0010). The Claude session that wrote the pull request (the *authoring session*) does not know the review is coming,
so it never answers it, and the owner is asked to review before the findings are dealt with.

The goal is small: after a push, the authoring session waits for the review, fixes the main findings, and asks the
owner to review once a review has no Major findings or after 3 reviews. A few tests cover the scripts. Mistakes found
in use are fixed afterwards; this is not meant to be a bulletproof mechanism.

## Analysis

### Why PR #57 failed

PR #57 built a wait script, locks and marker files around the reviewer (976 lines). In 17 review rounds the reviewer
found a new edge case in that machinery almost every time, and each fix added more. This design avoids new machinery.

### How the session waits

A session whose turn has ended is woken when a background task it started finishes. So the session itself runs
`scripts/review-pr.ps1` in the background after its push has finished. When the review is posted, the task ends and
the session is woken with the review on the task's output. No poll, no wait script.

Today the review hook starts the review. It fires when a background push *starts*, so it reads the PR before the push
lands and reviews the old commit (or skips it). Since the session now starts the review, the hook stops doing so.
Keeping both would run two reviews at once.

### What "main findings" means

The reviewer labels each finding Major or Minor and starts with a fixed verdict line, `**Verdict:** n Major, m Minor`.

* **Major:** wrong behavior on the normal path, a safety or hardware risk, a missing test for changed behavior, docs
  that state behavior the code does not have, or a solution clearly bigger than the issue needs.
* **Minor:** rare command sequences, wording, nice-to-haves.

The session reads the verdict itself. No script parses it.

### Fold push

Review fixes are `fixup!` commits. Before asking the owner to merge, the session folds them in
(`git rebase --autosquash`) and force-pushes. That push has a new commit SHA but the same code. `review-pr.ps1` skips
the review when the new head's code tree equals the tree of a commit it already reviewed on the same PR, and prints
that it did, so the waiting session ends at once.

### Spike results (2026-11-03, desktop app)

1. A background task that finishes after the turn has ended wakes the session. A 16-minute background task ran to the
   end and was not killed.
2. `claude -p` runs when started from inside a tool call.
3. The review hook fires when a background command starts, not when it ends. Its plain output does not reach the
   agent.

## Goals vs. Non-Goals

* **In-Scope Goals:**
  * `CLAUDE.md` tells the authoring session: after a push, run `scripts/review-pr.ps1` in the background; when it
    finishes, fix the Major findings (and cheap, local Minor ones) as fixup commits, push, and repeat. Stop when a
    review has no Major findings or after 3 reviews. Reply on the PR with what was fixed and what was skipped, fold
    the fixups, then ask the owner to review.
  * `review-pr.ps1`'s prompt asks for Major/Minor labels and the verdict line.
  * `review-pr.ps1` prints the posted review to its output.
  * `review-pr.ps1` skips a head whose tree was already reviewed on the PR, and says so.
  * The review hook no longer starts reviews: the hook and its `.claude/settings.json` entry are removed.
  * A short ADR amending ADR 0010; `scripts/README.md` updated.
* **Out-of-Scope Non-Goals:**
  * Wait scripts, locks, marker types beyond today's, decision tables, a script that parses the verdict.
  * Process-level fakes of `gh` and `claude`, manual or evaluation test tiers.
  * Linear issues for skipped Minor findings (they stay in the PR reply).
  * Reviews of pushes from a terminal or IDE (run `scripts/review-pr.ps1` by hand, as today).
  * [MAY-87: Need: review-pr.ps1 reports "nothing to review" when the branch has no pull request](https://linear.app/mayday-robot/issue/MAY-87).

## Testing

Pester tests in `scripts/Tests/ClaudeReview.Tests.ps1`, using the existing fakes for `gh`, `claude` and posting:

1. The reviewer prompt asks for Major/Minor labels and the verdict line.
2. `review-pr.ps1` prints the posted review to its output.
3. A head whose tree equals an already reviewed commit's tree is not reviewed again, and the script says so; a head
   with a different tree is reviewed.

Each is written first and seen failing against today's code. The hook's existing tests go with the hook.

The waiting and fixing behavior is exercised by use: this PR's own review loop is the check.

## Alternatives Weighed

* **Instruction in `CLAUDE.md` only, hook unchanged.** Rejected: nothing wakes the session when the review lands, and
  the hook reviews the old commit on a background push.
* **Desktop app PR monitor (Auto-fix).** Rejected: unverified for comments posted under the PR author's own login,
  and it needs approval per PR.
* **PR #57's wait script and markers.** Rejected: did not converge in 17 rounds.
* **A full decision table in the script, process-level fakes, manual tests.** Rejected by the owner as more than the
  issue needs; mistakes found in use get fixed afterwards.
* **No fold-push skip.** Rejected by the owner: an unchanged tree should not cost another review.

## Key Decisions & Staging for Next Steps

* The reviewer decides severity; the session fixes Majors and cheap Minors and skips the rest with a reason.
* Cap: 3 reviews per PR.
* The session starts the review; the hook is removed.
* The fold push is skipped when the tree is unchanged.
* Planned commits:
  1. `feat:` reviewer labels findings Major or Minor (prompt + test).
  2. `feat:` `review-pr.ps1` prints the posted review (+ test).
  3. `feat:` skip a head whose tree was already reviewed (+ test).
  4. `feat:` the authoring session runs and follows the review (`CLAUDE.md`, hook and settings entry removed, ADR,
     `scripts/README.md`).
  5. `docs:` record PR #57 as a failed attempt (via `record-knowledge`).
