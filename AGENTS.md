# Repository Guidance

## Project Purpose

Mayday is a custom C# robotics control library and experimentation platform for learning robotics and software architecture. It currently targets two robot platforms: Mayday, a hexapod, and Ellie, an articulated mobile robot. The software spans robot behavior, motion planning, kinematics, robot structures, actuator control, and hardware communication.

The system is intended to provide reliable abstractions for interacting with the physical world while keeping robot behavior testable and allowing hardware and algorithms to be replaced or experimented with independently.

## Start Here

Before making a significant change:

1. Read this file and the root `README.md`.
2. Read any `README.md` files in the affected project or namespace. At present, the repository also has `Ellie\EllieMain\README.md`; it contains only a heading, so use the source and project references to understand that area.
3. Inspect the affected project file, neighboring abstractions, callers, and relevant tests.
4. Trace dependencies across the layers before changing an API or moving a type.
5. For behavior that can affect physical hardware, understand its operating assumptions and prefer simulation or hardware-independent tests before running it on a robot.

Follow established patterns unless there is a clear architectural reason to change them. Avoid unrelated cleanup, formatting, or broad restructuring in a focused change.

## Solution Layout

`Mayday.sln` contains the main Mayday projects and the Ellie projects. Most projects target `net10.0`, enable nullable reference types and implicit usings, and use the SDK-style C# project format.

| Project or directory | Role |
|---|---|
| `RobotDomain` | Shared robot-domain foundation: geometry, links, joints, structures, motion abstractions, behavior abstractions, and timing/scheduling. Keep robot-specific concepts out of this layer. |
| `MaydayDomain` | Mayday hexapod structure, legs, postures, joint limits, and Mayday motion-planning and kinematics concepts. |
| `Dynamixel` | Dynamixel actuator adapter, register and unit conversions, driver, and communication bus implementations. |
| `ManualBehavior` | Manually selected Mayday behaviors, including terminal-driven posture and movement experiments. |
| `Robots` | Mayday robot assembly and factory/composition code that combines domain, behavior, and driver components. |
| `Main` | Mayday executable entry point and runtime startup/composition selection. |
| `MaydayDataAccess` | Mayday-specific persistence and mapping for data such as leg posture-by-position maps. |
| `Generic` | Shared utilities and system-facing terminal abstractions; keep it foundational and independent of robot-specific layers. |
| `Test` | Main xUnit project, separated into `Unit` and `Integration` tests, with test utilities and object mothers. |
| `Ellie\EllieMain` | Ellie-specific structure, behavior, motion planning, composition, and executable entry point. |
| `Ellie\EllieMainTests` | Ellie end-to-end tests. |
| `DataAccess`, `MauiApp1`, and `Ternimal` | Additional or experimental projects in the solution. Inspect their current code and references before relying on them as part of the primary robot runtime. |

The intended dependency direction follows the architecture described in the root README: stable robot abstractions at the core; robot-specific structure above them; motion planning and behavior above the structure; hardware adapters and concrete composition at the outer edges. The Mayday and Ellie applications are separate robot implementations that reuse shared concepts where appropriate. Avoid introducing dependencies from foundational abstractions toward robot-specific behavior, executable startup, or hardware libraries.

## Architecture and Namespace Boundaries

The system uses object-oriented domain abstractions together with functional-style transformations and composition in some projects. Preserve the distinction between:

- **Foundational abstractions** such as units-aware geometry, `Joint`, `Link`, motion plans, and timing in `RobotDomain`.
- **Robot-specific models** such as Mayday legs, structures, postures, and kinematics in `MaydayDomain`, or Ellie structures and planners in `Ellie\EllieMain`.
- **Behavior and application orchestration**, which chooses goals and connects planning to robot structures.
- **Adapters and infrastructure**, including Dynamixel communication and data access.
- **Composition roots**, notably `Main\Program.cs`, robot factories, and `Ellie\EllieMain\Program.cs`, which create concrete implementations and wire dependencies.

Types in a namespace root are part of that namespace's public API. Keep the public surface small and intentional. Put implementation details in `Internal` namespaces or corresponding internal folders when useful, and do not make other namespaces depend on those details. Prefer placing a type beside the concept it models rather than accumulating unrelated types in a broad utility namespace.

Dependencies should flow from volatile application and hardware code toward stable abstractions. Before adding a project reference, check whether an existing abstraction or project boundary is the right place for the dependency.

## C# Conventions

- Follow the surrounding file's style. Existing code commonly uses file-scoped namespaces, PascalCase type and public member names, camelCase locals and parameters, and underscore-prefixed private fields.
- Keep nullable annotations enabled and address compiler warnings rather than suppressing them. The solution treats warnings as errors in several core and test projects.
- Use domain types and units for physical quantities. Prefer `UnitsNet` values such as `Angle`, `Length`, and `RotationalSpeed` over unlabelled numeric values when working with physical measurements.
- Preserve domain value semantics and existing collection choices. Immutable collections are used in parts of the robot model; do not introduce mutable shared state without a clear ownership reason.
- Follow nearby use of records, primary constructors, collection expressions, `LanguageExt` effects/options, and LINQ rather than introducing a new style without need.
- Validate configuration and external inputs at boundaries. Make unsupported states explicit with established error or exception patterns instead of hiding failures behind defaults.
- Keep names consistent with the domain vocabulary already used in the relevant project. Avoid renaming public concepts as incidental cleanup.

## Composition and Dependency Injection

Composition roots own construction of concrete implementations and lifecycle resources. Pass dependencies explicitly, preferably through constructors, and make ownership and shutdown responsibilities clear. Avoid service locators, hidden global state, or hardware initialization inside domain logic.

Keep business and planning logic independent of how a concrete motor, terminal, clock, or persistence implementation is created. Use interfaces and abstractions at hardware and system boundaries so components can be substituted in tests, simulation, or a different robot build.

Factories in `Robots` and the robot-specific application entry points are natural places to assemble implementations. Extend the established composition flow rather than moving hardware setup into lower-level domain types.

## Hardware Abstraction and Safety

Hardware access is a core boundary. Use interfaces such as the Dynamixel `CommunicationBus` for device communication, and keep vendor/native library calls inside adapter implementations. Higher-level behavior, kinematics, and robot structures should depend on stable abstractions, not on native interop or a particular physical device.

- Keep hardware implementations replaceable and support hardware-independent tests wherever practical.
- Make operations that enable torque, move actuators, initialize devices, or open communication ports explicit.
- Validate IDs, configuration, units, and assumptions at boundaries; fail clearly on invalid configuration.
- Prefer predictable motion and explicit limits. Preserve joint limits, velocity limits, and other safety-related constraints when changing motion or driver behavior.
- Exercise hardware-dependent behavior in simulation or automated tests first when available. Tests marked with `PhysicalRobotFact` or `PhysicalRobotTheory` may require an actual robot and must not be treated as ordinary hardware-free tests.
- Do not run physical-hardware tests or startup programs casually; confirm the intended hardware, port, and operating conditions first.

## Threading and Timing

Preserve the existing periodic execution architecture unless a change has a strong, documented reason. The intended responsibilities are separated across startup/composition, behavior selection, motion planning, and actuator/joint driving. `RobotDomain.Time.PeriodicScheduler` provides periodic execution, while the driver and planner abstractions define their own work.

The joint driver can run at a higher frequency than behavior and planning updates. The Dynamixel communication bus serializes access to its underlying port with a lock, including operations that share native communication state. Do not bypass this boundary or make port access concurrent without establishing that the adapter and device protocol support it.

- Give mutable state a clear owner and minimize shared mutable state.
- Prefer immutable values and explicit handoff between periodic components.
- Keep concurrency boundaries and cancellation/shutdown behavior understandable.
- Avoid adding locks unless required; keep any lock scope as small as correctness permits.
- Document important thread-safety assumptions close to the affected API or implementation.
- Be cautious when changing scheduler cadence, timer behavior, cancellation, or process/thread priority: timing affects motion control and hardware communication.

## Testing

The codebase was developed using Outside-In Test Driven Development inspired by *Growing Object-Oriented Software, Guided by Tests*. Re-establish this workflow whenever practical:

1. Express the desired behavior with an end-to-end or integration test.
2. Add focused unit tests for important collaborators and edge cases.
3. Implement the smallest change that satisfies the behavior.

Tests are production assets. Prefer tests that describe observable behavior and preserve existing test intent unless behavior is deliberately changing. Use the existing xUnit conventions, including descriptive `Given...When...Then...` test names, and the assertion and mocking libraries already referenced by the test project (AwesomeAssertions and Moq).

The main test project groups tests under `Test\Unit` and `Test\Integration`; shared builders and configuration live under `Test\Utilities` and test object mother folders. Ellie end-to-end tests are under `Ellie\EllieMainTests\EndToEnd`. Reuse these patterns and helpers before adding new test infrastructure.

For behavior changes:

- Update or add tests for the public behavior, edge cases, and relevant failure paths.
- Use fakes, mocks, echo joints, or other hardware-independent collaborators where appropriate.
- Keep integration tests deterministic and avoid relying on wall-clock sleeps or real hardware unless the test is explicitly physical.
- Preserve testability of time-sensitive code by using the existing `TimeProvider` and timing abstractions where appropriate.

Useful commands from the repository root:

```powershell
dotnet test Test\Test.csproj
dotnet test Ellie\EllieMainTests\EllieMainTests.csproj
dotnet build Mayday.sln
```

Choose the smallest relevant test project first. Running the full solution can include ancillary or platform-specific projects, so target only what the change requires before escalating.

## Documentation

Documentation should explain behavior, purpose, and architectural intent rather than merely restating implementation. Keep implementation-specific details close to the implementation.

- Public methods should have XML documentation describing behavior, expectations, invariants, side effects, and contracts.
- Interfaces and significant classes should explain their purpose, responsibilities, architectural role, and behavioral semantics.
- Add or maintain a `README.md` in significant namespace or directory areas. Describe purpose, responsibilities, boundaries, dependency direction, primary abstractions, relationships to neighboring areas, and architectural rationale.
- Update relevant documentation when architecture or externally visible behavior changes.
- Keep examples and documentation consistent with the current code; do not copy outdated assumptions from old notes or experiments without checking them.

## Performance

Prioritize readability, maintainability, and correctness over micro-optimization. Do not optimize speculatively; identify and measure a real bottleneck first. If a less readable implementation is justified by measured performance needs, document the reason near the code.

Timing and motion loops are safety- and behavior-sensitive, so performance changes there must preserve scheduling and communication correctness and should be validated with appropriate measurements and tests.

## Change and Commit Discipline

Keep changes focused and reviewable. Separate structural refactoring from behavior changes whenever practical:

- A refactoring changes structure without changing observable behavior and should leave relevant tests passing.
- A behavior change alters features, algorithms, error handling, hardware capabilities, or robot movement; keep unrelated structural work out of that change.

Avoid mixing broad formatting, file moves, renames, and behavior changes. Prefer a sequence of small commits, each with one clear intent, so reviewers can understand, test, and revert changes independently. Tests and documentation should make the intent and architectural impact clear.
