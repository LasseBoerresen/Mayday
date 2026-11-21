# Test utilities

## Overview

Shared test infrastructure of the `Test` project (namespace `Test.Utilities`):
the xUnit attributes that mark physical, manual and quarantined tests, the
configuration they read, the yes/no prompt for manual tests, and test doubles
for `FatalErrorHandler` and `Terminal`. It tests no production code itself;
`Ellie\EllieMainTests` also uses it through its project reference to `Test`
(see [EllieMainTests](../../Ellie/EllieMainTests/README.md)).

## Interface

### `PhysicalRobotFactAttribute`

Marks a `[Fact]` that drives the real robot.
File: `PhysicalRobotFactAttribute.cs`.

| Member | What it does |
|---|---|
| Constructor | Sets `Skip` to `TestConfiguration.MaydayRobotIsNotClearToMoveReason` unless `TestConfiguration.IsRobotClearToMove()` is true. |
| Trait | `Tier=Physical`, reported by `PhysicalRobotTraitDiscoverer`. |

Used by `Test.Integration.Dynamixel.NativeSerialPortCommunicationBusTests` and
`Test.Integration.Main.MaydayRobotTests`.

### `PhysicalRobotTheoryAttribute`

The `[Theory]` form of `PhysicalRobotFactAttribute`, with the same skip rule
and the same trait.
File: `PhysicalRobotTheoryAttribute.cs`.

| Member | What it does |
|---|---|
| Constructor | Sets `Skip` to `MaydayRobotIsNotClearToMoveReason` unless `IsRobotClearToMove()` is true. |
| Trait | `Tier=Physical`, through `PhysicalRobotTraitDiscoverer`. |

Used by `Test.Integration.Dynamixel.RotationDirectionOnRobotTests` and
`Test.Integration.Main.MaydayLegTests`.

### `PhysicalRobotTraitDiscoverer`

Turns both physical attributes into the trait xUnit reports and filters on.
File: `PhysicalRobotFactAttribute.cs`.

| Member | What it does |
|---|---|
| `GetTraits(traitAttribute)` | Yields one trait, `Tier` = `Physical`. |

### `ManualRobotFactAttribute`

Marks a `[Fact]` that drives the real robot and asks the watching person,
through `HumanVerifier`, whether it did what the test expects.
File: `ManualRobotFactAttribute.cs`.

| Member | What it does |
|---|---|
| Constructor | Sets `Skip` to `MaydayRobotIsNotClearToMoveReason` if the robot is not clear to move; otherwise to `MaydayHumanIsNotPresentReason` if `TestConfiguration.IsHumanPresent()` is false. |
| Trait | `Tier=Manual`, reported by `ManualRobotTraitDiscoverer`. |

Used by `Test.Integration.Main.MaydayLegTests` and
`Test.Integration.MaydayDomain.MaydayStructureManualTests`.

### `ManualRobotTraitDiscoverer`

File: `ManualRobotFactAttribute.cs`.

| Member | What it does |
|---|---|
| `GetTraits(traitAttribute)` | Yields one trait, `Tier` = `Manual`. |

### `QuarantineAttribute`

Marks a test, or every test in a class, as known to fail for a stated reason.
It is not a test attribute itself: it goes beside `[Fact]`, `[Theory]` or one
of the attributes above.
File: `QuarantineAttribute.cs`.

| Member | What it does |
|---|---|
| Constructor `(reason)` | Stores `reason`. Allowed on classes and methods. |
| `Reason` | The reason given. |

Used by `Test.Integration.Main.MaydayLegTests` and
`Test.Unit.RobotDomain.Time.PeriodicSchedulerTests`.

### `QuarantineTraitDiscoverer`

File: `QuarantineAttribute.cs`.

| Member | What it does |
|---|---|
| `GetTraits(traitAttribute)` | Yields `Quarantine` = `true` and `QuarantineReason` = the first constructor argument. |

### `TestConfiguration`

Reads the switches that let a test touch the robot.
File: `TestConfiguration.cs`.

| Member | What it does |
|---|---|
| `Create()` | Builds a new `IConfiguration` from environment variables, then the Test project's user secrets (optional). |
| `IsRobotClearToMove()` | True only when `MAYDAY_ROBOT_IS_CLEAR_TO_MOVE` is exactly `True`. |
| `IsHumanPresent()` | True only when `MAYDAY_HUMAN_IS_PRESENT` is exactly `True`. |
| `MaydayRobotIsNotClearToMoveReason` | Skip message: ``Robot is not clear to move, set config key 'MAYDAY_ROBOT_IS_CLEAR_TO_MOVE' to 'true' to run this test.`` |
| `MaydayHumanIsNotPresentReason` | Skip message: ``No human is present to answer, set config key 'MAYDAY_HUMAN_IS_PRESENT' to 'True' to run this manual test.`` |

Only the attributes in this directory call it.

### `HumanVerifier`

Asks the person watching the robot a yes/no question, for manual tests whose
outcome only a person can see.
File: `HumanVerifier.cs`.

| Member | What it does |
|---|---|
| Constructor `(prompt)` | Takes the `HumanPrompt` to ask through. |
| `Confirms(question)` | Asks `"{question} (y/n)"` until the answer, trimmed and lower-cased, is `y`/`yes` (returns true) or `n`/`no` (returns false). Each repeat is prefixed with `Please answer y or n. `. |
| `NoAnswerException` | Thrown by `Confirms` when the prompt returns null. Message: `No answer was given to: {question}`. |

Used with `ConsoleWindowHumanPrompt(timeout: 10 minutes)` by
`Test.Integration.Main.MaydayLegTests` and
`Test.Integration.MaydayDomain.MaydayStructureManualTests`.

### `HumanPrompt`

Shows a question to a person and returns the line they type.
File: `HumanPrompt.cs`.

| Member | What it does |
|---|---|
| `Ask(question)` | Returns the answer, or null when none was given. |

### `ConsoleWindowHumanPrompt`

The `HumanPrompt` for real runs. It opens a separate PowerShell window, because
`dotnet test` and IDE runners redirect the test host's standard input.
File: `HumanPrompt.cs`.

| Member | What it does |
|---|---|
| Constructor `(timeout)` | How long `Ask` waits for an answer. |
| `Ask(question)` | Opens a window titled `Mayday manual test` that shows `question` and reads an `Answer`. Returns the typed text, or null when the window is closed without answering. |

`Ask` throws `PlatformNotSupportedException` when not on Windows,
`InvalidOperationException` when the window cannot be started, and
`TimeoutException` (`No answer within {timeout} to: {question}`) after killing
the window when the timeout passes.

### `RecordingFatalErrorHandler`

A `FatalErrorHandler` (from `RobotDomain.Time`) that records the exception
where production ends the process, so a failing periodic loop shows up as a
failed assertion instead of taking down the test host.
File: `RecordingFatalErrorHandler.cs`.

| Member | What it does |
|---|---|
| `Handle(exception)` | Adds `exception` to `Exceptions`. Never throws or exits. |
| `Exceptions` | The recorded exceptions, oldest first; safe to read from any thread. |

Used by `Test.Unit.Dynamixel.PeriodicallyBatchedJointDriverTests`,
`Test.Unit.RobotDomain.Time.PeriodicSchedulerTests` and Ellie's
`EndToEnd/EllieTests.cs`.

### `TestTerminal`

A scripted `Terminal` (from `Generic.Terminals`): it reads from a queue
instead of the keyboard and writes to the console.
File: `TestTerminal.cs`.

| Member | What it does |
|---|---|
| Constructor `(inputLines)` | Uses the given `Queue<string>` itself, not a copy. |
| `Write(value)` / `WriteLine(value)` | Writes to `Console`. |
| `ReadLine()` | Returns null when the queue is empty, without waiting. Otherwise dequeues the next line, echoes it with `Console.WriteLine` and returns it. |
| `InputLine(line)` | Adds `line` to the end of the queue. |

Used by `Test.Integration.Main.MaydayRobotTests` and Ellie's
`EndToEnd/EllieTests.cs`.

## Invariants

- A physical or manual test runs only when `MAYDAY_ROBOT_IS_CLEAR_TO_MOVE` is
  the exact, case-sensitive string `True`. A missing key, `False`, or `true`
  skips it. (The clear-to-move skip message says `'true'`, but only `True`
  passes the check.)
- A manual test also needs `MAYDAY_HUMAN_IS_PRESENT` to be exactly `True`, so
  an unattended physical run never waits for an answer. When both keys are
  missing, the skip reason is the clear-to-move one.
- User secrets override environment variables: `Create()` adds environment
  variables first and user secrets second, and the later source wins. An
  environment variable cannot switch a physical test on or off while the
  secrets hold the key
  ([root cause](../../knowledge/root-causes/user-secrets-override-environment.md)).
  So the gates keep physical and manual tests out by filtering on the traits,
  not only by configuration.
- The trait names and values (`Tier=Physical`, `Tier=Manual`,
  `Quarantine=true`) are what `scripts\gate.ps1` and
  `.github\workflows\ci.yml` filter on. Renaming them changes which tests the
  gates run.
- The discoverers are found by name (`"Test.Utilities.…TraitDiscoverer"`,
  assembly `"Test"`): moving or renaming them silently drops the traits.
- `HumanVerifier.Confirms` returns only for a clear yes or no; it asks again for
  anything else, so it can ask several times.
- Physical tests keep `FailFastFatalErrorHandler`; `RecordingFatalErrorHandler`
  is for hardware-free tests only.
- `TestTerminal` uses an unsynchronized `Queue<string>`.

## Side Effects

- The attribute constructors read configuration, through `TestConfiguration`:
  environment variables and the user-secrets file of `UserSecretsId`
  `de615b0d-591a-44e6-8040-6f9239d5d5dc`. Each call to `IsRobotClearToMove()`
  or `IsHumanPresent()` builds a new configuration.
- `ConsoleWindowHumanPrompt.Ask` creates a temporary directory
  (`mayday-human-prompt-*`) with `question.txt`, `ask.ps1` and, after an
  answer, `answer.txt`; starts `powershell.exe -NoProfile -ExecutionPolicy Bypass`
  with `UseShellExecute = true`, in a new window; blocks the calling thread until the window closes or the timeout
  passes; and deletes the directory afterwards.
- `TestTerminal` writes to the console.
- `HumanVerifier` and `RecordingFatalErrorHandler`: none beyond their prompt
  and their own list.

## Verification

- `HumanVerifier` has unit tests in [Unit/Utilities](../Unit/Utilities/README.md):
  `dotnet test Test\Test.csproj --filter "FullyQualifiedName~Test.Unit.Utilities"`.
  They use a scripted prompt and open no window.
- No test checks the attributes, their traits, `TestConfiguration`,
  `ConsoleWindowHumanPrompt`, `RecordingFatalErrorHandler` or `TestTerminal`
  directly. The tests listed above under each type use them.

## Related ADRs

[Quarantine](../../docs/adr/0004-quarantine-failing-tests-with-a-trait.md),
[test tiers](../../docs/adr/0005-test-tiers-for-physical-and-simulated-runs.md),
[manual robot tests](../../docs/adr/0009-manual-robot-tests-confirmed-by-a-human.md),
[pre-push gate drives the robot](../../docs/adr/0011-pre-push-gate-drives-the-robot.md).
