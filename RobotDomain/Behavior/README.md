# Shared behavior contracts

## Responsibility

Model robot behavior that holds for any robot: deciding what the robot should do, and starting it. Today it holds a start contract and a terminal-driven controller (see Known Issues).

## Interface

- **Contract:** `BehaviorController`, an interface with one method, `Unit Start()`, which starts the behavior (`BehaviorController.cs`).
- **Terminal control:** `TerminalBehaviorController<TCommand>(Terminal terminal, CancellationToken cancelToken)`, an abstract `BehaviorController` where `TCommand : struct, Enum` (`TerminalBehaviorController.cs`).
  - `Start()`: blocking; wakes up, prints each command as `<number>: <name>`, then reads, parses and executes commands, calling `PrintUpdate()` after each, until cancelled or input ends; returns `Unit.Default`.
  - Subclass hooks (protected abstract): `WakeUpBehavior()`, `SleepBehavior()`, `ExecuteCommand(TCommand)`, `PrintUpdate()`.
  - Protected helpers: `WakeUp()` and `Sleep()` print a line, then call their hook. `Start()` calls `WakeUp()`; nothing in this class calls `Sleep()`, so a subclass sleeps only when one of its commands calls it.

## Key Concepts

- **Layers:** a behavior decides *what* the robot does; planners and drivers decide *how* (see the layers of ports in [Motion](../Motion/README.md)).
- **Commands:** a subclass's `TCommand` enum is its menu. Each value can be typed by name, in any case, or by its number.
- **Injected terminal:** all input and output goes through the `Terminal` passed in, so tests and other front ends can replace the console.

## Dependencies

Part of `RobotDomain`, using nothing else in it. Uses:
- `Generic` for `Terminal`;
- `LanguageExt` for `Unit`.

## Dependency Rules

Do not reference robot-specific behavior implementations here.

## Invariants

Preserve cancellation and start lifecycle when implementing controllers.

- **Cancellation:** `Start()` checks `cancelToken` only between commands; the blocking `Terminal.ReadLine()` and `ExecuteCommand` do not observe it.
- **Input:** `ReadLine()` returning `null` (end of input) ends the loop. Input is parsed with `Enum.TryParse<TCommand>(…, ignoreCase: true)`. Invalid input prints `Invalid command: '…'` and prompts again.
- **Errors:** exceptions from the hooks propagate out of `Start()`; no hook is called on exit, cancellation or failure.

## Known Issues

- `TerminalBehaviorController` is a console menu, user interface rather than a domain concept: [MAY-109: Need: Terminal user interface lives outside RobotDomain](https://linear.app/mayday-robot/issue/MAY-109).
- Invalid input is retried by recursion, so a long run of it overflows the stack, and cancellation is not checked while retrying: [MAY-108: Need: TerminalBehaviorController keeps prompting after any number of invalid commands](https://linear.app/mayday-robot/issue/MAY-108).
- `HighLevelWalking.puml` describes one robot's gamepad walking, with types that no longer exist: [MAY-99: Need: RobotDomain code names no specific robot](https://linear.app/mayday-robot/issue/MAY-99).

## Side Effects

- Terminal I/O through the injected `Generic.System.Terminal` only (prompt, command list, wake-up/sleep and invalid-command lines); no direct `Console` access.
- `Start()` blocks the calling thread; this namespace starts no threads or tasks.

## Verification

No dedicated tests; the controllers are exercised only indirectly, through the robot factory and integration tests. Run `dotnet test Test\Test.csproj`.

## Architectural Constraints

Concrete terminal/hardware creation belongs at composition roots.

## Common Pitfalls

Do not reach devices or the console directly from a behavior; take them through the constructor.

## Related ADRs

[Stable abstractions](../../docs/adr/0001-stable-robot-abstractions.md).

## Related Findings

None recorded.

## Related Root Causes

None recorded.

## Related Lessons Learned

None recorded.

## Related Failed Attempts

None recorded.
