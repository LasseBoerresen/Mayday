# Make tests able to fail on the bug they guard

## Situation

`StepAngleTests` passed for the life of the project while `StepAngle` converted with 4,094 counts per revolution instead of the XL430's 4,096 ([root cause](../root-causes/step-angle-4094-counts.md)).

## Observation

Three things in the tests let the defect through. The tolerance was 1/4096 revolution, which is one count, the size of the error. The test data skipped step 0 and labelled step `1` as "-0.5 revolutions", so the broken mapping looked intended. And nothing round-tripped a value or tested both ends of the range. A round-trip over all 4,096 steps, once written, found the floating-point truncation (step 7 returned as step 6), which nobody had suspected.

## Recommendation

For a unit conversion, check against the vendor documentation instead of the existing tests. Use a tolerance well below the smallest meaningful step. Test both ends of the range, one step either side of each boundary, and a round trip across the whole domain when it is small enough to enumerate. When a test's data looks oddly offset, ask what it is encoding.

## Second case: a goal equal to the starting state

The quarantined `Given_WhenSetGoalToZeroAngle_...` test was stale three ways: it verified a single-joint `Write` although the periodic loop now writes all goals in one batch, it never called `Initialize`, so the joint was not in the state cache, and it set a zero-angle goal, which equals the `StepCenter` goal `Initialize` reads. Even repaired, it could not have told "wrote my goal" from "wrote the existing goal". The replacement uses a distinct 30 degree goal and asserts that the batch eventually contains that joint's expected steps, because the loop repeats the write every period. When a test sets a value, choose one that differs from the starting state.

## Related Components

[Dynamixel](../../Dynamixel/README.md), [RobotDomain/Motion](../../RobotDomain/Motion/README.md), [Test](../../Test/README.md)

## Related Tasks

[PR gating and test baseline](../task-summaries/pr-gating-and-test-baseline.md)
