# Manual robot tests confirmed by a human

## Status

Accepted.

## Context

Some robot behavior can only be checked by looking at the robot. Mayday has no sensor that reports where a leg tip is in the world: joint angles come back from the servos, but whether the model behind them matches the physical robot (mounting, rotation direction, posture map) cannot be read back. The echo model predicted that moving all tips forward from sitting sends the right center tip backward, while the owner saw the robot sway as expected. Only the person watching can settle that.

`PhysicalRobotFact` tests drive the robot but need an assertion the code can evaluate. The xUnit test host has no interactive console: `dotnet test` and IDE runners redirect its standard input, so a test cannot read a keyboard answer from its own console.

## Decision

Add a manual tier. `[ManualRobotFact]` marks a test that drives the real robot and asks the watching human a yes/no question through `HumanVerifier`, whose answer is the assertion.

- It carries the trait `Tier=Manual`, so `dotnet test --filter "Tier=Manual"` runs exactly these tests. This is the first use of the `Tier` trait proposed in [ADR 0005](0005-test-tiers-for-physical-and-simulated-runs.md).
- It skips unless both `MAYDAY_ROBOT_IS_CONNECTED` and `MAYDAY_HUMAN_IS_PRESENT` are `True`. The second key keeps an unattended physical run from waiting for an answer. The gate never runs them because its test filter excludes `Tier=Manual`; forcing `MAYDAY_ROBOT_IS_CONNECTED` to `False` is not enough, since the user secrets override the environment.
- `ConsoleWindowHumanPrompt` asks in a new PowerShell window and reads the answer back through a temporary file. `HumanVerifier` accepts `y`/`yes` and `n`/`no` and asks again on anything else; a closed window fails the test.
- A manual test checks its starting state with the human first, so a "no" in the end points at the behavior under test, not at the setup.

## Alternatives Considered

- Read the answer from the test host's console (`Console.ReadLine` or `CONIN$`): the host's input is redirected, and IDE runners have no console at all.
- A manual behavior in `ManualBehavior`, run from `Main`: it can move the robot but records no pass or fail, and is not discoverable or repeatable as a test.
- A sensor or camera that measures tip positions: the right long-term answer, not available today.

## Why

A recorded yes/no from the person at the robot is the only evidence of physical behavior available now. As an xUnit test it is repeatable, named for the behavior it checks, and selectable by trait.

## Consequences

- Benefits: physical behavior gets a test with a pass or fail; the prompt is testable without a human through the `HumanPrompt` seam.
- Costs: a human must be present, and each run takes the robot's real move time.
- Limitations: Windows only, since the prompt opens `powershell.exe`. The answer is subjective: questions must name what to look at and the size of the expected move. The window-opening prompt itself is exercised by use, not by an automated test.

## Related Components

[Test](../../Test/README.md), [Dynamixel](../../Dynamixel/README.md)

## Related ADRs

[0002](0002-hardware-adapter-boundary.md), [0005](0005-test-tiers-for-physical-and-simulated-runs.md), [0006](0006-run-quality-gates-locally.md)

## Related Findings

None recorded.

## Related Root Causes

None recorded.
