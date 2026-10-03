# Step angle conversion used 4,094 counts per revolution

## Symptoms

A goal or position at step 0 threw `ArgumentException: Angle bigger than a semicircle, got: '-0.5002442598925256 r'` from `StepAngle.ToSteps`. On the scheduler thread that ends the process ([test-host crash](test-host-crash-from-scheduler-failfast.md)). A round trip through `ToAngle` and `ToSteps` also changed some steps: step 7 came back as step 6.

## Investigation

- The thrown value equals `ToAngle(0)`: 2048 / 4094 = 0.50024426 revolutions.
- `Dynamixel/StepAngle.cs` set `StepExtreme = 2047` and `StepSize = Tau / (StepExtreme * 2)`, which is 4,094 counts per revolution.
- The Robotis e-Manual for the XL430-W250 gives Goal Position (116) and Present Position (132) a range of 0 to 4,095 by default, "4,096 counts" per revolution, 0.088 degrees per pulse. `Dynamixel/XL430_W250_control_table.csv` has Max Position Limit 4095 and Min Position Limit 0.
- The old `StepAngleTests` labelled step `1` as -0.5 revolutions and compared with a tolerance of 1/4096 revolution, which is one count, so no error of that size could fail.
- A round-trip test over every step 0 to 4,095 found the second defect: `ToSteps` truncated, and floating-point error in `angle / StepSize` produced 6.9999999 for step 7.

## Root Cause

The step size assumed 4,094 counts instead of 4,096, so every conversion was about 0.05% off, up to roughly one count (0.088 degrees) at the ends. It also made step 0 unrepresentable. Truncating, instead of rounding, made the round trip unreliable.

## Resolution

Pull request #26 (merged as `88cc705`) corrected the count to 4,096, rounds to the nearest step, and clamps at step 4,095. Tests were written first and failed against the old code.

It is a behavior change: angles to and from an actuator shift by up to about 0.09 degrees, and a goal is now within half a count of its target instead of up to one count below it. It was verified by unit tests only. It has not been run on the robot, and the owner chose not to run the physical check.

## Prevention

Test round trips across the whole range and at both ends, with a tolerance far below one count. Check conversions against the vendor documentation and the control table instead of the previous tests. See [the lesson](../lessons-learned/tests-must-discriminate.md).

## Related Components

[Dynamixel](../../Dynamixel/README.md), [Test](../../Test/README.md)

## Related Tasks

[PR gating and test baseline](../task-summaries/pr-gating-and-test-baseline.md)

## Related Decisions

[ADR 0002](../../docs/adr/0002-hardware-adapter-boundary.md)
